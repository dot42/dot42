using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.XModel;
using Dot42.Mapping;

namespace Dot42.CompilerLib.Structure
{
    public interface IClassBuilder
    {
        /// <summary>
        /// Sorting low comes first
        /// </summary>
        int SortPriority { get; }

        /// <summary>
        /// Gets fullname of the underlying type.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets the (abstracted) type for which a class is build.
        /// </summary>
        XTypeDefinition XType { get; }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        void Create(ITargetPackage targetPackage);


        /// <summary>
        /// Implement the class now that all classes have been created
        /// </summary>
        void Implement(ITargetPackage targetPackage);

        /// <summary>
        /// Implement make minor fixes after the implementation phase.
        /// </summary>
        void FixUp(ITargetPackage targetPackage);

        /// <summary>
        /// Generate code for all methods.
        /// </summary>
        void GenerateCode(ITargetPackage targetPackage);

        /// <summary>
        /// Create all annotations for this class and it's members
        /// </summary>
        void CreateAnnotations(ITargetPackage targetPackage);

        /// <summary>
        /// Record the mapping from .NET to Dex
        /// </summary>
        void RecordMapping(MapFile mapFile);
    }
}
