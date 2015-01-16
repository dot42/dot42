using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dot42.Compiler.Shared;
using Dot42.CompilerLib;
using Dot42.CompilerLib.XModel.DotNet;
using Mono.Cecil;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const string AppWidgetProviderAttribute = "AppWidgetProviderAttribute";
        private static readonly EnumFormatter widgetResizeModesOptions = new EnumFlagsFormatter(
            Tuple.Create(0x0001, "none"),
            Tuple.Create(0x0002, "horizontal"),
            Tuple.Create(0x0004, "vertical")
            );
        private static readonly EnumFormatter widgetCategoriesOptions = new EnumFormatter(
            Tuple.Create(1, "home_screen"),
            Tuple.Create(2, "keyguard")
            );

        /// <summary>
        /// Create all activity elements
        /// </summary>
        private void CreateAppWidgetProvider(XElement application, string outputFolder)
        {
            //Debugger.Launch();
            // Create activities
            var index = -1;
            foreach (var tuple in FindAppWidgetProviders())
            {
                index++;
                var type = tuple.Item1;
                var xType = XBuilder.AsTypeDefinition(module, type);
                var attr = tuple.Item2;

                var receiver = new XElement("receiver");
                application.Add(receiver);

                // receiver attributes
                receiver.AddAttr("name", Namespace, FormatClassName(xType));
                receiver.AddAttrIfNotEmpty("label", Namespace, attr.GetValue<string>("Label"), FormatStringOrLiteral);

                // intent-filter
                CreateIntentFilter(receiver, type, false, true);

                // meta-data
                receiver.Add(new XElement("meta-data",
                    new XAttribute(XName.Get("name", Namespace), "android.appwidget.provider"),
                    new XAttribute(XName.Get("resource", Namespace), "@xml/" + AppWidgetProviderResource.GetResourceName(index))));
                CreateMetaData(receiver, type);

                // Create the appwidget-provider xml file
                CreateAppWidgetProviderFile(outputFolder, index, attr);
            }

            // Check that the number of app widgets is correct
            if (index > appWidgetProviderCodeFiles.Count)
            {
                throw new CompilerException("For more AppWidgetProvider attributes than source files with subtype AppWidgetProvider");
            }
        }

        /// <summary>
        /// Find all activities with their Activity attribute.
        /// </summary>
        private IEnumerable<Tuple<TypeDefinition, CustomAttribute>> FindAppWidgetProviders()
        {
            return assembly.MainModule.GetTypes()
                           .Select(x => Tuple.Create(x, x.GetAttributes(AppWidgetProviderAttribute).FirstOrDefault()))
                           .Where(x => x.Item2 != null);
        }

        /// <summary>
        /// Create app widget provider xml file.
        /// </summary>
        private void CreateAppWidgetProviderFile(string tempFolder, int index, CustomAttribute attr)
        {
            var resourceName = AppWidgetProviderResource.GetResourceName(index);
            var path = Path.Combine(Path.Combine(tempFolder, @"res\xml"), resourceName + ".xml");

            var doc = new XDocument();
            var root = new XElement("appwidget-provider");
            doc.Add(root);

            root.AddAttrIfNotEmpty("minWidth", Namespace, attr.GetValue<string>("MinWidth"));
            root.AddAttrIfNotEmpty("minHeight", Namespace, attr.GetValue<string>("MinHeight"));
            root.AddAttrIfNotDefault("updatePeriodMillis", Namespace, attr.GetValue<long>("UpdatePeriod"), 0L);
            root.AddAttrIfNotEmpty("previewImage", Namespace, attr.GetValue<string>("PreviewImage"), FormatDrawable);
            root.AddAttrIfNotEmpty("initialLayout", Namespace, attr.GetValue<string>("InitialLayout"), FormatLayout);
            var configureActivityType = attr.GetValue<TypeReference>("ConfigureActivity");
            if (configureActivityType != null)
            {
                var configureActivityTypeDef = configureActivityType.Resolve();
                if (configureActivityTypeDef == null)
                    throw new ArgumentException("Cannot resolve " + configureActivityType.FullName);
                root.AddAttr("configure", Namespace, FormatClassName(XBuilder.AsTypeDefinition(module, configureActivityTypeDef)));
            }
            root.AddAttrIfNotDefault("resizeMode", Namespace, attr.GetValue<int>("ResizeMode"), 0, widgetResizeModesOptions.Format);
            root.AddAttrIfNotDefault("widgetCategory", Namespace, attr.GetValue<int>("Category"), 0, widgetCategoriesOptions.Format);
            root.AddAttrIfNotEmpty("initialKeyguardLayout", Namespace, attr.GetValue<string>("InitialKeyguardLayout"), FormatLayout);

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            doc.Save(path);
        }
    }
}
