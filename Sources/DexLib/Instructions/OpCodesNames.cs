using System.Linq;

namespace Dot42.DexLib.Instructions
{
    public static partial class OpCodesNames
    {
        /// <summary>
        /// Gets the name for the given value
        /// </summary>
        public static string GetName(OpCodes value)
        {
            var name = Names.Where(x => x.Item1 == (int) value).Select(x => x.Item2).FirstOrDefault();
            return name ?? "0x" + ((int) value).ToString("X2");
        }
    }
}
