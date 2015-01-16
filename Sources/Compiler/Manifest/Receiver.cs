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
        private const string ReceiverAttribute = "ReceiverAttribute";

        /// <summary>
        /// Create all receiver elements
        /// </summary>
        private void CreateReceiver(XElement application)
        {
            // Create receivers
            foreach (var tuple in FindReceivers())
            {
                var type = tuple.Item1;
                var xType = XBuilder.AsTypeDefinition(module, type);
                var attr = tuple.Item2;

                var receiver = new XElement("receiver");
                application.Add(receiver);

                receiver.AddAttr("name", Namespace, FormatClassName(xType));
                receiver.AddAttrIfNotEmpty("label", Namespace, attr.GetValue<string>("Label"), FormatStringOrLiteral);
                receiver.AddAttrIfNotEmpty("icon", Namespace, attr.GetValue<string>("Icon"), FormatDrawable);
                receiver.AddAttrIfFound("enabled", Namespace, attr, "Enabled");
                receiver.AddAttrIfFound("exported", Namespace, attr, "Exported");
                receiver.AddAttrIfNotEmpty("permission", Namespace, attr.GetValue<string>("Permission"));
                receiver.AddAttrIfNotEmpty("process", Namespace, attr.GetValue<string>("Process"));

                CreateIntentFilter(receiver, type, false, false);
                CreateMetaData(receiver, type);
            }
        }

        /// <summary>
        /// Find all receivers with their Receiver attribute.
        /// </summary>
        private IEnumerable<Tuple<TypeDefinition, CustomAttribute>> FindReceivers()
        {
            return assembly.MainModule.GetTypes()
                           .Select(x => Tuple.Create(x, x.GetAttributes(ReceiverAttribute).FirstOrDefault()))
                           .Where(x => x.Item2 != null);
        }
    }
}
