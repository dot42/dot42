using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Dot42.ImportJarLib.Doxygen;

namespace Dot42.ImportJarLib.Model
{
    [DebuggerDisplay("{@Name} {@PropertyTypes}")]
    public sealed class NetPropertyDefinition : INetMemberDefinition
    {
        private readonly List<NetParameterDefinition> parameters = new List<NetParameterDefinition>();
        private readonly List<NetCustomAttribute> customAttributes = new List<NetCustomAttribute>();

        /// <summary>
        /// Name of the type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Human readable description.
        /// </summary>
        public DocDescription Description { get; set; }

        /// <summary>
        /// The editor browsable state of this member
        /// </summary>
        public EditorBrowsableState EditorBrowsableState { get; set; }

        /// <summary>
        /// Parent (in case of nested types)
        /// </summary>
        public NetTypeDefinition DeclaringType { get; set; }

        /// <summary>
        /// Type of the property
        /// </summary>
        public NetTypeReference PropertyType { get { return Getter.ReturnType; } }

        /// <summary>
        /// All parameters
        /// </summary>
        public List<NetParameterDefinition> Parameters { get { return parameters; } }

        /// <summary>
        /// Gets all custom attributes
        /// </summary>
        public List<NetCustomAttribute> CustomAttributes { get { return customAttributes; } }

        /// <summary>
        /// Get method
        /// </summary>
        public NetMethodDefinition Getter { get; set; }

        /// <summary>
        /// Set method
        /// </summary>
        public NetMethodDefinition Setter { get; set; }
    }
}
