using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Mono.Cecil;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const string IntentFilterAttribute = "IntentFilterAttribute";
        private const string ActionMain = "android.intent.action.MAIN";
        private const string ActionAppWidgetUpdate = "android.appwidget.action.APPWIDGET_UPDATE";
        private const string CategoryLauncher = "android.intent.category.LAUNCHER";

        /// <summary>
        /// Create all activity elements
        /// </summary>
        private void CreateIntentFilter(XElement parent, TypeDefinition type, bool visibleInLauncher, bool appWidget)
        {
            // Create intent filter
            var attr = type.GetAttributes(IntentFilterAttribute).FirstOrDefault();
            if ((attr == null) && !(visibleInLauncher || appWidget)) 
                return;

            var intentFilter = new XElement("intent-filter");
            parent.Add(intentFilter);

            intentFilter.AddAttrIfNotEmpty("label", Namespace, attr.GetValue<string>(-1, "Label"));
            intentFilter.AddAttrIfNotEmpty("icon", Namespace, attr.GetValue<string>(-1, "Icon"), FormatDrawable);

            var actionsArr = (attr != null) ? attr.GetValue<string[]>("Actions") : null;
            var categoriesArr = (attr != null) ? attr.GetValue<string[]>("Categories") : null;

            var actions = new List<string>(actionsArr ?? Enumerable.Empty<string>());
            var categories = new List<string>(categoriesArr ?? Enumerable.Empty<string>());
            if (visibleInLauncher && !actions.Contains(ActionMain)) 
                actions.Insert(0, ActionMain);
            if (appWidget && !actions.Contains(ActionAppWidgetUpdate))
                actions.Insert(0, ActionAppWidgetUpdate);
            if (visibleInLauncher && !categories.Contains(CategoryLauncher))
                categories.Insert(0, CategoryLauncher);

            foreach (var action in actions)
            {
                intentFilter.Add(new XElement("action", new XAttribute(XName.Get("name", Namespace), action)));
            }
            foreach (var cat in categories)
            {
                intentFilter.Add(new XElement("category", new XAttribute(XName.Get("name", Namespace), cat)));
            }
        }
    }
}
