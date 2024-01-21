﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gluino.SourceGeneration;

[Generator]
[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "TypeScript type definition generation")]
public class BindingGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
    
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            return;

        var bindingWindows = receiver.WindowClasses
            .Select(cds => new BindingWindow(context.Compilation, cds))
            .ToArray();

        GenerateWindowPartial(context, bindingWindows);
        GenerateTypeScript(context, bindingWindows);
    }

    private static void GenerateWindowPartial(GeneratorExecutionContext context, BindingWindow[] bindingWindows)
    {
        foreach (var bwnd in bindingWindows) {
            var usingsBuilder = new StringBuilder();
            var builder = new StringBuilder();
            var ns = bwnd.Symbol.ContainingNamespace.ToDisplayString();

            builder
                .AppendLine($"namespace {ns}")
                .AppendLine("{")
                .AppendLine($"    {bwnd.Symbol.DeclaredAccessibility.ToDisplayString()} partial class {bwnd.Name}")
                .AppendLine("    {")
                .AppendLine("        protected override void InitializeBindings()")
                .AppendLine("        {");

            foreach (var method in bwnd.Methods) {
                foreach (var p in method.Parameters) {
                    var pns = p.Type.ContainingNamespace.ToDisplayString();
                    if (pns != ns) {
                        usingsBuilder.AppendLine($"using {pns};");
                    }
                }

                builder.AppendLine($"            WebView.Bind(\"{method.CustomName}\", {method.Name}, {method.Global.ToString().ToLower()});");
            }

            builder
                .AppendLine("        }")
                .AppendLine("    }")
                .AppendLine("}");

            var contents = $"{(usingsBuilder.Length > 0 ? usingsBuilder.AppendLine() : "")}{builder}";

            context.AddSource($"{bwnd.Name}.g", contents);
        }
    }

    private static void GenerateTypeScript(GeneratorExecutionContext context, BindingWindow[] bindingWindows)
    {
        var indexBuilder = new StringBuilder();

        foreach (var bwnd in bindingWindows) {
            var nonPrimitives = bwnd.Methods.SelectMany(m => m.GetNonPrimitives());
            foreach (var typeSymbol in nonPrimitives) {
                indexBuilder.AppendLine(typeSymbol.ToTypeScriptInterface()).AppendLine();
            }
        }
        
        foreach (var bwnd in bindingWindows) {
            indexBuilder.AppendLine(bwnd.ToTypeScript());
        }
        
        indexBuilder.AppendLine()
            .AppendLine("declare global {")
            .AppendLine("  interface Gluino {")
            .AppendLine("    sendMessage: (msg: string) => void;")
            .AppendLine("    addListener: (callback: (msg: string) => void) => void;")
            .AppendLine("    removeListener: (callback: (msg: string) => void) => void;")
            .AppendLine("    invoke: (name: string, args: any[], callback: (result: any) => void) => void;")
            .AppendLine("    bindings: {");

        foreach (var bwnd in bindingWindows) {
            indexBuilder.AppendLine($"      {bwnd.Name.ToCamelCase()}: {bwnd.Name};");
        }

        foreach (var gbm in bindingWindows.SelectMany(bwnd => bwnd.Methods).Where(m => m.Global)) {
            indexBuilder.AppendLine($"      {gbm.ToTypeScript()}");
        }

        indexBuilder
            .AppendLine("    }")
            .AppendLine("  }").AppendLine()
            .AppendLine("  interface Window {")
            .AppendLine("    gluino: Gluino;")
            .AppendLine("  }")
            .AppendLine("}");

        var tsDir = Path.Combine(context.GetCallingPath(), "TypeScript");
        var tsIndexFile = Path.Combine(tsDir, "index.d.ts");
        var tsPackageFile = Path.Combine(tsDir, "package.json");

        if (!Directory.Exists(tsDir))
            Directory.CreateDirectory(tsDir);

        File.WriteAllText(tsIndexFile, indexBuilder.ToString());
        File.WriteAllText(tsPackageFile,
            $$"""
              {
                "name": "gluino-types",
                "version": "{{context.GetCallingVersion() ?? "1.0.0"}}",
                "main": "index.js",
                "types": "index.d.ts"
              }
              """);
    }

    public class BindingWindow
    {
        public BindingWindow(Compilation comp, ClassDeclarationSyntax cds)
        {
            var model = comp.GetSemanticModel(cds.SyntaxTree);
            var symbol = ModelExtensions.GetDeclaredSymbol(model, cds) as INamedTypeSymbol;

            Name = cds.Identifier.Text;
            Methods = cds.Members.OfType<MethodDeclarationSyntax>()
                .Where(mds => mds.AttributeLists
                    .Any(als => als.Attributes
                        .Any(a => a.Name.ToString() == "Binding")))
                .Select(mds => new BindingMethod(comp, mds))
                .ToArray();

            Symbol = symbol;
        }

        public readonly string Name;
        public readonly BindingMethod[] Methods;
        public readonly INamedTypeSymbol Symbol;

        public string ToTypeScript()
        {
            var builder = new StringBuilder()
                .AppendLine($"export interface {Name} {{");

            foreach (var method in Methods) {
                if (method.Global) continue;
                builder.AppendLine($"  {method.ToTypeScript()}");
            }

            builder.AppendLine("}");

            return builder.ToString().TrimEnd();
        }
    }

    public class BindingMethod
    {
        public BindingMethod(Compilation comp, MethodDeclarationSyntax mds)
        {
            var model = comp.GetSemanticModel(mds.SyntaxTree);
            var symbol = ModelExtensions.GetDeclaredSymbol(model, mds) as IMethodSymbol;
            var attr = symbol?.GetAttribute("Gluino", "BindingAttribute");

            Name = symbol?.Name ?? mds.Identifier.Text;
            CustomName = attr?.GetArgumentValue<string>("Name") ?? Name.ToCamelCase();
            Global = attr?.GetArgumentValue<bool>("Global") ?? false;

            Parameters = symbol?.Parameters.ToArray();
            ReturnType = symbol?.ReturnType;
        }

        public readonly string Name;
        public readonly string CustomName;
        public readonly bool Global;
        public readonly IParameterSymbol[] Parameters;
        public readonly ITypeSymbol ReturnType;

        public string ToTypeScript()
        {
            var ret = ReturnType.ToTypeScript();
            var args = string.Join(", ", Parameters.Select(p => $"{p.Name}: {p.Type.ToTypeScript()}"));
            var sig = $"{CustomName}: ({args}) => Promise<{ret}>;";
            return sig;
        }

        public INamedTypeSymbol[] GetNonPrimitives()
        {
            var list = new List<INamedTypeSymbol>();

            if (!ReturnType.IsPrimitive())
                list.Add(ReturnType as INamedTypeSymbol);

            var paramTypes = Parameters
                .Select(p => p.Type)
                .OfType<INamedTypeSymbol>()
                .Where(t => !t.IsPrimitive());
            list.AddRange(paramTypes);

            return [.. list];
        }
    }

    private class SyntaxReceiver : ISyntaxReceiver
    {
        public readonly List<ClassDeclarationSyntax> WindowClasses = [];

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not ClassDeclarationSyntax cds)
                return;
            
            if (cds.Inherits("Window")) {
                WindowClasses.Add(cds);
            }
        }
    }
}

public static class Extensions
{
    public static string ToTypeScript(this ITypeSymbol typeSymbol)
    {
        if (!typeSymbol.IsPrimitive() && typeSymbol is INamedTypeSymbol namedTypeSymbol) {
            if (namedTypeSymbol.IsGenericType &&
                namedTypeSymbol.AllInterfaces.Any(i => i.ToDisplayString() == "System.Collections.Generic.IEnumerable`1") &&
                namedTypeSymbol.TypeArguments.Length == 1) {
                return $"{namedTypeSymbol.TypeArguments[0].ToTypeScript()}[]";
            }

            return namedTypeSymbol.Name;
        }

        if (typeSymbol.TypeKind == TypeKind.Array && typeSymbol is IArrayTypeSymbol arrayTypeSymbol) {
            return $"{arrayTypeSymbol.ElementType.ToTypeScript()}[]";
        }

        if (typeSymbol.TypeKind is TypeKind.Class or TypeKind.Struct) {
            switch (typeSymbol.SpecialType) {
                case SpecialType.System_String: return "string";
                case SpecialType.System_Boolean: return "boolean";
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_IntPtr:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_UIntPtr:
                case SpecialType.System_Decimal:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                    return "number";
                case SpecialType.System_Void: return "void";
                default: return "any";
            }
        }

        return "any";
    }

    public static string ToTypeScriptInterface(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            return null;

        var interfaceBuilder = new StringBuilder()
            .AppendLine($"export interface {namedTypeSymbol.Name} {{");
        var members = namedTypeSymbol
            .GetMembers()
            .Where(s => s.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal && !s.IsStatic);

        foreach (var member in members) {
            var type = member switch {
                IFieldSymbol { IsReadOnly: false, IsConst: false } fieldSymbol => fieldSymbol.Type,
                IPropertySymbol { IsReadOnly: false, IsIndexer: false } propSymbol => propSymbol.Type,
                _ => null
            };
            if (type == null) continue;

            interfaceBuilder.AppendLine($"  {member.Name.ToCamelCase()}: {type.ToTypeScript()};");
        }

        interfaceBuilder.AppendLine("}");

        return interfaceBuilder.ToString().TrimEnd();
    }

    public static bool Inherits(this ClassDeclarationSyntax cds, string typeName)
    {
        return cds.BaseList != null &&
               cds.BaseList.Types
                   .Any((t) => t.Type is IdentifierNameSyntax ins && ins.Identifier.Text == typeName);
    }

    public static bool IsPrimitive(this ITypeSymbol type)
    {
        return type != null && type.SpecialType != SpecialType.None;
    }

    public static AttributeData GetAttribute(this ISymbol symbol, string name)
    {
        return symbol
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == name);
    }

    public static AttributeData GetAttribute(this ISymbol symbol, string namespaceName, string attributeName)
    {
        return symbol
            .GetAttributes()
            .FirstOrDefault(a =>
                a.AttributeClass?.ContainingNamespace?.ToDisplayString() == namespaceName &&
                a.AttributeClass?.Name == attributeName);
    }

    public static T GetArgumentValue<T>(this AttributeData attributeData, string name, T defaultValue = default)
    {
        var dict = attributeData.NamedArguments.ToDictionary(kv => kv.Key, kv => kv.Value);
        if (!dict.TryGetValue(name, out var tc)) return defaultValue;

        if (typeof(T).IsEnum)
            return (T)Enum.Parse(typeof(T), tc.Value?.ToString() ?? "None");

        return (T)tc.Value;
    }

    public static string GetArgumentValueCSharp(this AttributeData attributeData, string name)
    {
        var dict = attributeData.NamedArguments.ToDictionary(kv => kv.Key, kv => kv.Value);
        return !dict.TryGetValue(name, out var tc) ? null : tc.ToCSharpString();
    }

    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length == 1)
            return value.ToLowerInvariant();

        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    public static string GetCallingPath(this GeneratorExecutionContext context)
    {
        return context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.projectdir", out var result) ? result : null;
    }

    public static string GetCallingVersion(this GeneratorExecutionContext context)
    {
        return !context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.PackageVersion", out var packageVersion) ? null : packageVersion;
    }

    public static string ToDisplayString(this Accessibility accessibility)
    {
        return accessibility switch {
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            _ => "public"
        };
    }
}