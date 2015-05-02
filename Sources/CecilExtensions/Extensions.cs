using System;
using System.Globalization;
using Mono.Cecil;

namespace Dot42.CecilExtensions
{
    public static partial class Extensions
    {
        public static string ToScopeId(this MetadataToken token)
        {
            return token.ToUInt32().ToString("x8", CultureInfo.InvariantCulture);
        }
    }
}
