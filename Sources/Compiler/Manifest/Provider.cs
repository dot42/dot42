using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Dot42.CompilerLib.XModel.DotNet;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const string ProviderAttribute = "ProviderAttribute";
       
        /// <summary>
        /// Create all provider elements
        /// </summary>
        private void CreateProvider(XElement manifest)
        {
            // Create services
            foreach (var tuple in FindProviders())
            {
                var type = tuple.Item1;
                var xType = XBuilder.AsTypeDefinition(module, type);
                var attr = tuple.Item2;

                var provider = new XElement("provider");
                manifest.Add(provider);

                provider.AddAttr("name", Namespace, FormatClassName(xType));
                var authoritiesArr = (attr != null) ? attr.GetValue<string[]>("Authorities") : null;
                if(authoritiesArr != null && authoritiesArr.Any())
                {
                    //semicolon seperated list
                    provider.AddAttr("authorities", Namespace, string.Join(";", authoritiesArr)); 
                }
                provider.AddAttrIfFound("enabled", Namespace, attr, "Enabled");
                provider.AddAttrIfFound("exported", Namespace, attr, "Exported");
                provider.AddAttrIfFound("grantUriPermissions", Namespace, attr, "GrantUriPermissions");
                provider.AddAttrIfNotEmpty("icon", Namespace, attr.GetValue<string>("Icon"), FormatDrawable);
                provider.AddAttrIfNotEmpty("label", Namespace, attr.GetValue<string>("Label"), FormatStringOrLiteral);
                provider.AddAttrIfFound("multiprocess", Namespace, attr, "MultiProcess");
                provider.AddAttrIfNotEmpty("permission", Namespace, attr.GetValue<string>("Permission"));
                provider.AddAttrIfNotEmpty("process", Namespace, attr.GetValue<string>("Process"));
                provider.AddAttrIfNotEmpty("readPermission", Namespace, attr.GetValue<string>("ReadPermission"));
                provider.AddAttrIfFound("syncable", Namespace, attr, "Syncable");
                provider.AddAttrIfNotEmpty("writePermission", Namespace, attr.GetValue<string>("WritePermission"));
            }
        }

        /// <summary>
        /// Find all providers with their Providers attribute.
        /// </summary>
        private IEnumerable<Tuple<TypeDefinition, CustomAttribute>> FindProviders()
        {
            return assembly.MainModule.GetTypes()
                .Select(x => Tuple.Create(x, x.GetAttributes(ProviderAttribute).FirstOrDefault()))
                .Where(x => x.Item2 != null);
        }
    }
}
