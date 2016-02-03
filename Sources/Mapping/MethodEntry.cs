using System.Diagnostics;
using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.Mapping
{
    /// <summary>
    /// Method entry in map file
    /// </summary>
    [DebuggerDisplay("{Name}{Signature}")]
    public sealed class MethodEntry
    {
        private readonly string name;
        private readonly string signature;
        private readonly string dexName;
        private readonly string dexSignature;
        private readonly int mapFileId;
        private readonly VariableList variables;
        private readonly ParameterList parameters;
        private readonly string scopeId;

        /// <summary>
        /// Default ctor
        /// </summary>
        public MethodEntry(string name, string signature, string dexName, string dexSignature, int mapFileId, string scopeId=null)
        {
            this.name = name;
            this.signature = signature;
            this.dexName = dexName;
            this.dexSignature = dexSignature;
            this.mapFileId = mapFileId;
            this.scopeId = scopeId;
            variables  = new VariableList();
            parameters = new ParameterList();
        }

        /// <summary>
        /// XML ctor
        /// </summary>
        internal MethodEntry(XElement e)
        {
            name = e.GetAttribute("name");
            signature = e.GetAttribute("signature");
            dexName = e.GetAttribute("dname");
            dexSignature = e.GetAttribute("dsignature");
            mapFileId = int.Parse(e.GetAttribute("id") ?? "0");
            scopeId = e.GetAttribute("scopeid");
            variables = new VariableList(e);
            parameters = new ParameterList(e);
        }

        /// <summary>
        /// Convert to XML element
        /// </summary>
        internal XElement ToXml(string elementName)
        {
            var e = new XElement(elementName,
                new XAttribute("name", name),
                new XAttribute("signature", signature),
                new XAttribute("dname", dexName),
                new XAttribute("dsignature", dexSignature),
                new XAttribute("id", mapFileId.ToString()));

            if (scopeId != null) e.Add(new XAttribute("scopeid", scopeId));

            variables.AddTo(e);
            parameters.AddTo(e);
            return e;
        }

        /// <summary>
        /// Method name in .NET
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Method signature in .NET
        /// </summary>
        public string Signature { get { return signature; } }

        /// <summary>
        /// Method name in Dex
        /// </summary>
        public string DexName { get { return dexName; } }

        /// <summary>
        /// Method signature in Dex
        /// </summary>
        public string DexSignature { get { return dexSignature; } }

        /// <summary>
        /// Unique id of this entry in the map file.
        /// </summary>
        public int Id { get { return mapFileId; } }

        /// <summary>
        /// Unique id of this entry in its scope (as defined by the type it belongs to).
        /// </summary>
        public string ScopeId { get { return scopeId; } }

        /// <summary>
        /// All local variables
        /// </summary>
        public VariableList Variables { get { return variables; } }

        /// <summary>
        /// All method parameters
        /// </summary>
        public ParameterList Parameters { get { return parameters; } }

        /// <summary>
        /// Is there a match with the given parameters.
        /// </summary>
        public bool IsDexMatch(string dexName, string dexSignature)
        {
            return
                (dexName == this.dexName) &&
                (dexSignature == this.dexSignature);
        }
    }
}
