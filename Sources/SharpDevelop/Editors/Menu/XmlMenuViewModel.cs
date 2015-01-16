
using System;
using System.Reflection;
using System.Xml.Linq;
using Dot42.Ide.Editors;
using Dot42.Ide.Editors.Menu;
using Dot42.Ide.Serialization;
using Dot42.Ide.Serialization.Nodes.Menu;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace Dot42.SharpDevelop.Editors.Menu
{
	/// <summary>
	/// Description of XmlMenuViewModel.
	/// </summary>
	internal class XmlMenuViewModel : XmlViewModel, IMenuViewModel
	{
        private readonly AppResourceSerializer serializer;
        private SerializationNode root;

        /// <summary>
        /// Default ctor
        /// </summary>
        public XmlMenuViewModel(OpenedFile file) : base(file)
        {
            serializer = new AppResourceSerializer();
        }
        		
		/// <summary>
		/// Call NotifyPropertyChanged with the name of the root property.
		/// </summary>
		protected override void OnViewModelChanged() 
		{
			NotifyPropertyChanged("Menu");
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
