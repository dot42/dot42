using Mono.Cecil;

namespace Dot42.ImportJarLib
{
    internal class FrameworkAssemblyResolver : IAssemblyResolver
    {
        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return null;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            return Resolve(name);
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            return Resolve(AssemblyNameReference.Parse(fullName));
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            return Resolve(AssemblyNameReference.Parse(fullName), parameters);
        }
    }
}
