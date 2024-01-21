namespace Gluino;

/// <summary>
/// Indicates that a binding should be created for the method.<br/>
/// Does the same thing as <see cref="WebView.Bind"/> except TypeScript definitions will be generated.<br/>
/// The generated TypeScript definitions are written to a folder named <c>TypeScript</c> in your project folder.
/// </summary>
/// <remarks>
/// <example>
/// The C# method:
/// <code>
/// [Binding]
/// public void MyMethod() { /*...*/ }
/// </code>
/// Can be called like so in JavaScript:
/// <code>
/// // The funciton exists in <c>window.gluino.bindings</c> within an object representing the window.
/// // The Window's derivative name will always be converted to camelCase.
/// // The method name is converted to camelCase by default, but can be overridden in the <see cref="BindingAttribute"/>.
/// const { myMethod } = window.gluino.bindings.myWindow; 
/// myMethod();
/// </code>
/// </example>
/// <para>
/// This attribute will only work if used on a method inside a derivative of <see cref="Window"/> that uses the <see langword="partial"/> modifier.
/// Example:
/// <example>
/// <code>
/// public partial class MyWindow : Window
/// </code>
/// </example>
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class BindingAttribute : Attribute
{
    /// <summary>
    /// Override of the name given to the JavaScript function representing the bound C# method.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Places the function in the <c>window.gluino.bindings</c> instead of placing it in the scope of the window in which it was created.
    /// </summary>
    /// <remarks>
    /// <example>
    /// <code>
    /// const { myMethod } = window.gluino.bindings;
    /// myMethod();
    /// </code>
    /// </example>
    /// </remarks>
    public bool Global { get; set; }
}
