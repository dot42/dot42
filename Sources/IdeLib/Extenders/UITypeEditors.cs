using Dot42.ResourcesLib;

namespace Dot42.Ide.Extenders
{
    public class LanguageEditor : StringListUITypeEditor
    {
        public LanguageEditor() : base(ConfigurationQualifiers.CountryCodes)
        {
        }
    }

    public class RegionEditor : StringListUITypeEditor
    {
        public RegionEditor()
            : base(ConfigurationQualifiers.RegionCodes)
        {
        }
    }
}
