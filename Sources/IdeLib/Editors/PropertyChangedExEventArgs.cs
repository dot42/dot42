using System.ComponentModel;

namespace Dot42.Ide.Editors
{
    public class PropertyChangedExEventArgs : PropertyChangedEventArgs
    {
        public PropertyChangedExEventArgs(string propertyName, bool isModelProperty)
            : base(propertyName)
        {
            IsModelProperty = isModelProperty;
        }

        public bool IsModelProperty
        {
            get;
            private set;
        }
    }

    public static class PropertyChangedExtensions
    {
        public static bool IsModelProperty(this PropertyChangedEventArgs template)
        {
            var exTemplate = template as PropertyChangedExEventArgs;
            return (exTemplate == null) || exTemplate.IsModelProperty;
        }        
    }
}
