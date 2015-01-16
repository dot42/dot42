using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dot42.BuildTools.Documentation
{
    internal class DocumentationBuilder
    {
        private readonly string frameworksFolder;
        private readonly string outputFolder;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DocumentationBuilder(string frameworksFolder, string outputFolder)
        {
            this.frameworksFolder = frameworksFolder;
            this.outputFolder = outputFolder;
        }

        /// <summary>
        /// Generate the documentation
        /// </summary>
        public void Generate()
        {
            //Debugger.Launch();
            var list = new List<FrameworkDocumentationSet>();
            foreach (var folder in Directory.GetDirectories(frameworksFolder))
            {
                Console.WriteLine("Loading {0}", Path.GetFileName(folder));
                var framework = new FrameworkDocumentationSet(folder);
                framework.LoadAll();
                list.Add(framework);
            }

            Console.WriteLine("Generating docs");
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);
            var frameworks = list.OrderBy(x => x.Version).ToList();
            frameworks.Last().Generate(frameworks, outputFolder);
        }
    }
}
