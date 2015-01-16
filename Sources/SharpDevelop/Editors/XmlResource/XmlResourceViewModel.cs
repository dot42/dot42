using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;
using Dot42.Ide.Editors;
using Dot42.Ide.Editors.XmlResource;
using Dot42.Ide.Serialization;
using Dot42.Ide.Serialization.Nodes.XmlResource;
using ICSharpCode.SharpDevelop;

namespace Dot42.SharpDevelop.Editors.XmlResource
{
    /// <summary>
    /// View model describing an android XML resource
    /// </summary>
    internal class XmlResourceViewModel : XmlViewModel, IResourceViewModel
    {
        private readonly AppResourceSerializer serializer;
        private SerializationNode root;

        /// <summary>
        /// Default ctor
        /// </summary>
        public XmlResourceViewModel(OpenedFile file) : base(file)
        {
            serializer = new AppResourceSerializer();
        }
        		
		/// <summary>
		/// Call NotifyPropertyChanged with the name of the root property.
		/// </summary>
		protected override void OnViewModelChanged() 
		{
			NotifyPropertyChanged("Resources");
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
        /// Gets the resources root.
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public ResourcesNode Resources
        {
            get { return Root as ResourcesNode; }
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
