using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dot42.CompilerLib.XModel.DotNet;
using Mono.Cecil;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const string ServiceAttribute = "ServiceAttribute";

        /// <summary>
        /// Create all service elements
        /// </summary>
        private void CreateService(XElement application)
        {
            // Create services
            foreach (var tuple in FindServices())
            {
                var type = tuple.Item1;
                var xType = XBuilder.AsTypeDefinition(module, type);
                var attr = tuple.Item2;

                var service = new XElement("service");
                application.Add(service);

                service.AddAttr("name", Namespace, FormatClassName(xType));
                service.AddAttrIfNotEmpty("label", Namespace, attr.GetValue<string>("Label"), FormatStringOrLiteral);
                service.AddAttrIfNotEmpty("icon", Namespace, attr.GetValue<string>("Icon"), FormatDrawable);
                service.AddAttrIfFound("enabled", Namespace, attr, "Enabled");
                service.AddAttrIfFound("exported", Namespace, attr, "Exporter");
                service.AddAttrIfFound("isolatedProcess", Namespace, attr, "IsolatedProcess");
                service.AddAttrIfNotEmpty("permission", Namespace, attr.GetValue<string>("Permission"));
                service.AddAttrIfNotEmpty("process", Namespace, attr.GetValue<string>("Process"));
                service.AddAttrIfFound("stopWithTask", Namespace, attr, "StopWithTask");

                CreateIntentFilter(service, type, false, false);
                CreateMetaData(service, type);
            }
        }

        /// <summary>
        /// Find all services with their Service attribute.
        /// </summary>
        private IEnumerable<Tuple<TypeDefinition, CustomAttribute>> FindServices()
        {
            return assembly.MainModule.GetTypes()
                .Select(x => Tuple.Create(x, x.GetAttributes(ServiceAttribute).FirstOrDefault()))
                .Where(x => x.Item2 != null);
        }
    }
}
