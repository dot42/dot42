using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dot42.WcfTools.Extensions;
using Mono.Cecil;

namespace Dot42.WcfTools.ProxyBuilder
{
    /// <summary>
    /// Build C# proxy code for all ServiceContract interfaces found in the given assemblies.
    /// </summary>
    public sealed class ProxyBuilderTool
    {
        private readonly List<AssemblyDefinition> assemblies;
        private readonly string generatedSourcePath;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ProxyBuilderTool(IEnumerable<AssemblyDefinition> assemblies, string generatedSourcePath)
        {
            this.generatedSourcePath = generatedSourcePath;
            this.assemblies = assemblies.ToList();
        }

        /// <summary>
        /// Build all proxies
        /// </summary>
        public void Build()
        {
            // Collect all types that are ServiceContract interfaces
            var serviceContractIntfTypes = assemblies.SelectMany(x => x.MainModule.GetTypes()).Where(x => x.IsInterface && x.HasServiceContractAttribute());
            var proxyBuilders = serviceContractIntfTypes.Select(x => new ProxyClassBuilder(x)).ToList();

            // Build structures
            var context = new ProxySerializationContext();
            proxyBuilders.ForEach(x => x.Create(context));
            context.Create(context);

            // Generate code
            var code = new StringBuilder(128 * 1024);
            using (var codeWriter = new StringWriter(code))
            {
                var codeGenerator = new CodeGenerator();
                proxyBuilders.ForEach(x => x.Generate(codeGenerator));
                context.Generate(codeGenerator);
                codeGenerator.Generate(codeWriter);
            }

            // Load existing C# code
            var existingCode = File.Exists(generatedSourcePath) ? File.ReadAllText(generatedSourcePath) : string.Empty;
            var generatedCode = code.ToString();
            if (existingCode != generatedCode)
            {
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(generatedSourcePath));

                // Write C#
                File.WriteAllText(generatedSourcePath, generatedCode);
            }
        }
    }
}
