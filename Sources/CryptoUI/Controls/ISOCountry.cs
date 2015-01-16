using System;
using System.Collections.Generic;
using System.IO;

namespace Dot42.CryptoUI.Controls
{
    internal sealed class ISOCountry
    {
        private readonly string name;
        private readonly string code;
        private static ISOCountry[] list;

        private ISOCountry(string name, string code)
        {
            this.name = name;
            this.code = code;
        }

        /// <summary>
        /// Gets the full country name (in english) 
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Gets the 2-letter country code 
        /// </summary>
        public string Code { get { return code; } }

        /// <summary>
        /// Convert to a string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Code);
        }

        /// <summary>
        /// Get all countries.
        /// </summary>
        public static ISOCountry[] List
        {
            get { return list ?? (list = LoadCountries()); }
        }

        /// <summary>
        /// Load the list of countries.
        /// </summary>
        /// <returns></returns>
        private static ISOCountry[] LoadCountries()
        {
            var list = new List<ISOCountry>();
            try
            {
                var type = typeof(ISOCountry);
                using (var reader = new StringReader(Strings.Countries))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        int semiColon = line.IndexOf(';');
                        list.Add(new ISOCountry(line.Substring(0, semiColon), line.Substring(semiColon + 1)));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("Cannot load country table: {0}\n{1}", ex.Message, ex.StackTrace));
            }
            return list.ToArray();
        }
    }
}
