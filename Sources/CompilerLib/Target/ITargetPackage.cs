using Dot42.Mapping;

namespace Dot42.CompilerLib.Target
{
    public interface ITargetPackage
    {
        /// <summary>
        /// Compile all methods to the target platform.
        /// </summary>
        void CompileToTarget(bool generateDebugInfo, MapFile mapFile);

        /// <summary>
        /// Called after all methods have been compiled.
        /// </summary>
        void AfterCompileMethods();

        /// <summary>
        /// Verify the target package before it is being saved.
        /// </summary>
        void VerifyBeforeSave(string freeAppsKey);

        /// <summary>
        /// Save the target package to the given (already existing) output folder.
        /// </summary>
        void Save(string outputFolder);
    }
}
