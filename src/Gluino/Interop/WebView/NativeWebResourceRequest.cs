using System.Runtime.InteropServices;

namespace Gluino.Interop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal struct NativeWebResourceRequest
{
    public string Url;
    public string Method;
}
