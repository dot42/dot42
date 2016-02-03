using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.Utility;
using Mono.Cecil;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const string ApplicationAttribute = "ApplicationAttribute";
        private static readonly EnumFormatter uiOptions = new EnumFormatter(Tuple.Create(0, "none"), Tuple.Create(1, "splitActionBarWhenNarrow"));

        /// <summary>
        /// Create the application element
        /// </summary>
        private void CreateApplication(Targets target, XElement manifest, string outputFolder)
        {
            // Find application attribute
            var appTuple = FindApplication();
            var attr = appTuple.Item2;

            var label = attr.GetValue<string>(0, "Label");
            if (string.IsNullOrEmpty(label))
                throw new ArgumentException(string.Format("No Label set in {0}", attr));
            var icon = attr.GetValue<string>("Icon");
            if (string.IsNullOrEmpty(icon))
            {
                // Select icon from activity
                var activityIcons = FindActivities().Select(x => x.Item2.GetValue<string>("Icon")).Where(x => !string.IsNullOrEmpty(x));
                icon = activityIcons.FirstOrDefault();
            }
            // Create application
            var application = new XElement("application");
            manifest.Add(application);
            application.AddAttr("label", Namespace, FormatStringOrLiteral(label));
            if (appTuple.Item1 != null)
            {
                var xType = XBuilder.AsTypeDefinition(module, appTuple.Item1);
                application.AddAttr("name", Namespace, FormatClassName(xType));
            }
            else
            {
                // use Dot42.Internal.Application
                // FIXME: there should be a better way to specify the type...
                application.AddAttr("name", Namespace, "dot42.Internal.Application");
            }
            application.AddAttrIfNotEmpty("icon", Namespace, icon, FormatDrawable);
            application.AddAttrIfNotEmpty("theme", Namespace, attr.GetValue<string>("Theme"), FormatStyle);
            application.AddAttrIfNotEmpty("description", Namespace, attr.GetValue<string>("Description"));
            application.AddAttrIfNotEmpty("logo", Namespace, attr.GetValue<string>("Logo"), FormatDrawable);
            application.AddAttrIfNotDefault("debuggable", Namespace, attr.GetValue<bool>("Debuggable", debuggable), false);
            application.AddAttrIfFound("enabled", Namespace, attr, "Enabled");
            application.AddAttrIfNotDefault("persistent", Namespace, attr.GetValue<bool>("Persistent"), false);
            application.AddAttrIfFound("allowTaskReparenting", Namespace, attr, "AllowTaskReparenting");
            application.AddAttrIfNotEmpty("backupAgent", Namespace, attr.GetValue<Type>("BackupAgent"), nsConverter.GetConvertedFullName);
            application.AddAttrIfFound("hardwareAccelerated", Namespace, attr, "HardwareAccelerated");
            application.AddAttrIfFound("killAfterRestore", Namespace, attr, "KillAfterRestore");
            application.AddAttrIfFound("largeHeap", Namespace, attr, "LargeHeap");
            application.AddAttrIfNotEmpty("manageSpaceActivity", Namespace, attr.GetValue<Type>("ManageSpaceActivity"), nsConverter.GetConvertedFullName);
            application.AddAttrIfNotEmpty("process", Namespace, attr.GetValue<string>("Process"));
            application.AddAttrIfFound("restoreAnyVersion", Namespace, attr, "RestoreAnyVersion");
            application.AddAttrIfNotEmpty("taskAffinity", Namespace, attr.GetValue<string>("TaskAffinity"));
            application.AddAttrIfNotDefault("uiOptions", Namespace, attr.GetValue<int>("UIOptions"), 0, uiOptions.Format);

            // Create child elements
            CreateActivity(application);
            CreateService(application);
            CreateReceiver(application);
            CreateAppWidgetProvider(application, outputFolder);
            CreateUsesLibrary(application);
            CreateProvider(application);
            CreateMetaData(application, assembly); // Must be last
        }

        /// <summary>
        /// Find a custom Application class or assembly level Application attribute. all activities with their Activity attribute.
        /// </summary>
        private Tuple<TypeDefinition, CustomAttribute> FindApplication()
        {
            // Find custom application types.
            var appTypes = assembly.MainModule.GetTypes()
                                   .Select(x => Tuple.Create(x, x.GetAttributes(ApplicationAttribute).FirstOrDefault()))
                                   .Where(x => x.Item2 != null).ToList();
            if (appTypes.Count > 1)
            {
                throw new ArgumentException("Multiple custom Application classes found.");
            }

            // Find assembly level application attributes.
            var appAttributes = assembly.GetAttributes(ApplicationAttribute).ToList();
            if ((appAttributes.Count == 0) && (appTypes.Count == 0))
                throw new ArgumentException("Application attribute not found");
            if (appAttributes.Count > 1)
                throw new ArgumentException("Multiple Application attributes found");
            if ((appAttributes.Count > 0) && (appTypes.Count > 0))
                throw new ArgumentException("Found both an assembly level Application attribute as well as a custom Application type");

            if (appTypes.Count > 0)
                return appTypes[0];

            return Tuple.Create<TypeDefinition, CustomAttribute>(null, appAttributes[0]);
        }
    }
}
