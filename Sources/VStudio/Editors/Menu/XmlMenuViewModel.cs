using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;
using Dot42.Ide.Editors;
using Dot42.Ide.Editors.Menu;
using Dot42.Ide.Serialization;
using Dot42.Ide.Serialization.Nodes.Menu;
using Dot42.VStudio.XmlEditor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Dot42.VStudio.Editors.Menu
{
    /// <summary>
    /// View model describing an android XML menu
    /// </summary>
    internal class XmlMenuViewModel : XmlViewModel, IMenuViewModel
    {
        private readonly AppResourceSerializer serializer;
        private SerializationNode root;

        /// <summary>
        /// Default ctor
        /// </summary>
        public XmlMenuViewModel(IXmlStore xmlStore, IXmlModel xmlModel, IServiceProvider provider, IVsTextLines buffer)
            : base(xmlStore, xmlModel, provider, buffer)
        {
            serializer = new AppResourceSerializer();
        }

        /// <summary>
        /// Load the model from the underlying text buffer.
        /// </summary>
        protected override void LoadModelFromXmlModel(XDocument document)
        {
            Root = serializer.Deserialize(document);
        }

        /// <summary>
        /// This method is called when it is time to save the designer values to the underlying buffer.
        /// </summary>
        protected override XDocument SaveModelToXmlModel()
        {
            return serializer.Serialize(Root);
        }

        /// <summary>
        /// Gets the menu root.
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public MenuNode Menu
        {
            get { return Root as MenuNode; }
        }

        /// <summary>
        /// Gets access to the serialization root
        /// </summary>
        public override SerializationNode Root
        {
            get { return root; }
            set
            {
                if (root != value)
                {
                    if (root != null)
                    {
                        root.PropertyChanged -= OnRootPropertyChanged;
                    }
                    root = value;
                    if (value != null)
                    {
                        value.PropertyChanged += OnRootPropertyChanged;
                    }
                }
            }
        }

        private void OnRootPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.IsModelProperty())
            {
                DesignerDirty = true;
            }
            NotifyPropertyChanged(e.PropertyName);
        }
    }
}
