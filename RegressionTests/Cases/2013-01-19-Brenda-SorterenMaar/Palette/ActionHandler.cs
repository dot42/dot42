using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.View;

namespace SorterenMaar.Palette
{
  public interface IDragDropActionHandlerActionHandler
  {
  }

  public abstract class DragDropActionHandler
  {
    DragDropActionHandler successor = null;
    public DragDropActionHandler Successor 
    {
      get { return successor; }
      set { successor = value; } 
    }

    //public abstract bool HandleAction(View view, DragEvent dragEvent);

  }
}
