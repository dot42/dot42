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
        private const string ActivityAttribute = "ActivityAttribute";
        private static readonly EnumFormatter configChangesOptions = new EnumFlagsFormatter(
            Tuple.Create(0x0001, "mmc"), 
            Tuple.Create(0x0002, "mnc"),
            Tuple.Create(0x0004, "locale"),
            Tuple.Create(0x0008, "touchscreen"),
            Tuple.Create(0x0010, "keyboard"),
            Tuple.Create(0x0020, "keyboardHidden"),
            Tuple.Create(0x0040, "navigation"),
            Tuple.Create(0x0080, "screenLayout"),
            Tuple.Create(0x0100, "fontScale"),
            Tuple.Create(0x0200, "uiMode"),
            Tuple.Create(0x0400, "orientation"),
            Tuple.Create(0x0800, "screenSize"),
            Tuple.Create(0x1000, "smallestScreenSize")
            );
        private static readonly EnumFormatter launchModesOptions = new EnumFormatter(
            Tuple.Create(0, "standard"),
            Tuple.Create(1, "singleTop"),
            Tuple.Create(2, "singleTask"),
            Tuple.Create(3, "singleInstance")
            );
        private static readonly EnumFormatter screenOrientationsOptions = new EnumFormatter(
            Tuple.Create(0, "unspecified"),
            Tuple.Create(1, "user"),
            Tuple.Create(2, "behind"),
            Tuple.Create(3, "landscape"),
            Tuple.Create(4, "portrait"),
            Tuple.Create(5, "reverseLandscape"),
            Tuple.Create(6, "reversePortrait"),
            Tuple.Create(7, "sensorLandscape"),
            Tuple.Create(8, "sensorPortrait"),
            Tuple.Create(9, "sensor"),
            Tuple.Create(10, "fullSensor"),
            Tuple.Create(11, "nonensor")
            );

        private static readonly EnumFormatter windowSoftInputModeOptions = new WindowSoftInputModeFormatter();

        /// <summary>
        /// Create all activity elements
        /// </summary>
        private void CreateActivity(XElement application)
        {
            bool isFirst = true;

            // Create activities
            foreach (var tuple in FindActivities())
            {
                var type = tuple.Item1;
                var xType = XBuilder.AsTypeDefinition(module, type);
                var attr = tuple.Item2;

                var activity = new XElement("activity");
                application.Add(activity);

                activity.AddAttr("name", Namespace, FormatClassName(xType));
                activity.AddAttrIfNotEmpty("label", Namespace, attr.GetValue<string>("Label"), FormatStringOrLiteral);
                activity.AddAttrIfNotEmpty("icon", Namespace, attr.GetValue<string>("Icon"), FormatDrawable);
                activity.AddAttrIfFound("allowTaskReparenting", Namespace, attr, "AllowTaskReparenting");
                activity.AddAttrIfFound("alwaysRetainTaskState", Namespace, attr, "AlwaysRetainTaskState");
                activity.AddAttrIfFound("clearTaskOnLaunch", Namespace, attr, "ClearTaskOnLaunch");
                activity.AddAttrIfNotDefault("configChanges", Namespace, attr.GetValue<int>("ConfigChanges"), 0, configChangesOptions.Format);
                activity.AddAttrIfFound("enabled", Namespace, attr, "Enabled");
                activity.AddAttrIfFound("excludeFromRecents", Namespace, attr, "ExcludeFromRecents");
                activity.AddAttrIfFound("exported", Namespace, attr, "Exported");
                activity.AddAttrIfFound("finishOnTaskLaunch", Namespace, attr, "FinishOnTaskLaunch");
                activity.AddAttrIfFound("hardwareAccelerated", Namespace, attr, "HardwareAccelerated");
                activity.AddAttrIfNotDefault("launchMode", Namespace, attr.GetValue<int>("LaunchMode"), 0, launchModesOptions.Format);
                activity.AddAttrIfFound("multiprocess", Namespace, attr, "MultiProcess");
                activity.AddAttrIfFound("noHistory", Namespace, attr, "NoHistory");
                activity.AddAttrIfNotEmpty("parentActivityName", Namespace, attr.GetValue<Type>("ParentActivity"), nsConverter.GetConvertedFullName);
                activity.AddAttrIfNotEmpty("permission", Namespace, attr.GetValue<string>("Permission"));
                activity.AddAttrIfNotEmpty("process", Namespace, attr.GetValue<string>("Process"));
                activity.AddAttrIfNotDefault("screenOrientation", Namespace, attr.GetValue<int>("ScreenOrientation"), 0, screenOrientationsOptions.Format);
                activity.AddAttrIfFound("stateNotNeeded", Namespace, attr, "StateNotNeeded");
                activity.AddAttrIfNotEmpty("taskAffinity", Namespace, attr.GetValue<string>("TaskAffinity"));
                activity.AddAttrIfNotEmpty("theme", Namespace, attr.GetValue<string>("Theme"), FormatStyle);
                activity.AddAttrIfNotDefault("uiOptions", Namespace, attr.GetValue<int>("UIOptions"), 0, uiOptions.Format);
                activity.AddAttrIfNotDefault("windowSoftInputMode", Namespace, attr.GetValue<int>("WindowSoftInputMode"), 0, windowSoftInputModeOptions.Format);

                var visibleInLauncher = isFirst 
                                     || attr.GetValue("VisibleInLauncher", false) 
                                     || attr.GetValue("MainLauncher", false);

                CreateIntentFilter(activity, type, visibleInLauncher, false);
                CreateMetaData(activity, type);

                isFirst = false;
            }
        }

        /// <summary>
        /// Find all activities with their Activity attribute.
        /// </summary>
        private IEnumerable<Tuple<TypeDefinition, CustomAttribute>> FindActivities()
        {
            return assembly.MainModule.GetTypes()
                .Select(x => Tuple.Create(x, x.GetAttributes(ActivityAttribute).FirstOrDefault()))
                .Where(x => x.Item2 != null)
                .OrderByDescending(x => x.Item2.GetValue("MainLauncher", false))
                .ThenByDescending(x => x.Item2.GetValue("VisibleInLauncher", false));
        }
    }
}
