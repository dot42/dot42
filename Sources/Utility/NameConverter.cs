using System.Linq;

namespace Dot42.Utility
{
    public static class NameConverter
    {
        /// <summary>
        /// Convert to upper camel case.
        /// </summary>
        public static string UpperCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            var parts = name.Split(new[] { '.' });
            for (var i = 0; i < parts.Length; i++)
            {
                var p = parts[i];
                if (char.IsLower(p, 0))
                {
                    parts[i] = char.ToUpper(p[0]) + p.Substring(1);
                }
            }

            return (parts.Length == 1) ? parts[0] : string.Join(".", parts);
        }

        /// <summary>
        /// Convert to upper camel case.
        /// </summary>
        public static string UpperCamelCase(string name, params string[] convertToAllUppercase)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            var parts = name.Split(new[] { '.' });
            for (var i = 0; i < parts.Length; i++)
            {
                var p = parts[i];

                if (convertToAllUppercase.Contains(p))
                {
                    parts[i] = parts[i].ToUpperInvariant();
                }
                else if (char.IsLower(p, 0))
                {
                    parts[i] = char.ToUpper(p[0]) + p.Substring(1);
                }
            }

            return (parts.Length == 1) ? parts[0] : string.Join(".", parts);
        }
    }
}
