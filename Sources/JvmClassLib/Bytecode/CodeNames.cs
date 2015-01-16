using System.Linq;

namespace Dot42.JvmClassLib.Bytecode
{
    public static partial class CodeNames
    {
        /// <summary>
        /// Gets the name for the given value
        /// </summary>
        public static string GetName(Code value)
        {
            var name = Names.Where(x => x.Item1 == (int) value).Select(x => x.Item2).FirstOrDefault();
            return name ?? "0x" + ((int) value).ToString("X2");
        }
    }
}
