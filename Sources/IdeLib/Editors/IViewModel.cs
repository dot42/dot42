using System;
using System.ComponentModel;
using Dot42.Ide.Serialization;

namespace Dot42.Ide.Editors
{
    public interface IViewModel : INotifyPropertyChanged
    {
        bool DesignerDirty { get; set; }
        
        //event EventHandler ViewModelChanged;
        void DoIdle();
        void Close();

        /// <summary>
        /// Gets the root element
        /// </summary>
        ////SerializationNode Root { get; }
    }
}