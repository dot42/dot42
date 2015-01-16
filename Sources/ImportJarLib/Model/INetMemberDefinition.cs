using System.ComponentModel;
using Dot42.ImportJarLib.Doxygen;

namespace Dot42.ImportJarLib.Model
{
    public interface INetMemberDefinition
    {
        /// <summary>
        /// Parent (in case of nested types)
        /// </summary>
        NetTypeDefinition DeclaringType { get; set; }

        /// <summary>
        /// Name of the member
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Human readable description.
        /// </summary>
        DocDescription Description { get; set; }

        /// <summary>
        /// The editor browsable state of this member
        /// </summary>
        EditorBrowsableState EditorBrowsableState { get; set; }
    }
}
