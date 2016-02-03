using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.XModel;
using Dot42.Mapping;

namespace Dot42.CompilerLib.Structure
{
    /// <summary>
    /// Class builder that does nothing
    /// </summary>
    internal class SkipClassBuilder : IClassBuilder
    {
        public int SortPriority { get { return 0; } }

        public string FullName { get; private set; }
        public XTypeDefinition XType { get; private set; }

        public void Create(ITargetPackage targetPackage)
        {
        }

        public void Implement(ITargetPackage targetPackage)
        {
        }

        public void FixUp(ITargetPackage targetPackage)
        {
        }

        public void GenerateCode(ITargetPackage targetPackage, bool stopAtFirstError)
        {
        }

        public void CreateAnnotations(ITargetPackage targetPackage)
        {
        }

        public void RecordMapping(MapFile mapFile)
        {
        }
    }
}
