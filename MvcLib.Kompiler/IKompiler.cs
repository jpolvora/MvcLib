using System.Collections.Generic;

namespace MvcLib.Kompiler
{
    public interface IKompiler
    {
        string CompileFromSource(Dictionary<string, string> files, out byte[] buffer);
        string CompileFromFolder(string folder, out byte[] buffer);
        string CompileString(string text, out byte[] buffer);
    }
}