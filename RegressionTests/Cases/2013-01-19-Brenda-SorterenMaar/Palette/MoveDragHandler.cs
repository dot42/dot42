using System;
using Android.Content;
using Android.View;
using SorterenMaar.Games;

namespace SorterenMaar.Palette
{
  public class MoveDragData : IDragData
  {
    public View draggedView;
    public MoveDragHandler dragHandler;
  }

  public class MoveDragHandler : DragDropActionHandler, View.IOnDragListener, View.IOnLongClickListener, View.IOnTouchListener
  {
    bool inDrag = false;

    private string currentContainer= "default";

	  public string CurrentContainer
	  {
		  get { return currentContainer;}
		  set { currentContainer = value;}
	  }
	
    public ICheckerData CheckerData { get; set; }


    public bool OnTouch(View view, MotionEvent ev)
    {
      //if (ev.GetAction() == MotionEvent.ACTION_MOVE)
      {
        ClipData data = ClipData.NewPlainText("DragData", "");
        view.StartDrag(data,
                       new Android.View.View.DragShadowBuilder(view),
                       (Object)new MoveDragData { draggedView = view, dragHandler = this }, 0);
        
      }
      return true;
    }

    public bool OnLongClick(View view)
    {
      ClipData data = ClipData.NewPlainText("DragData", "");
      view.StartDrag(data, 
                     new Android.View.View.DragShadowBuilder(view), 
                     (Object)new MoveDragData{ draggedView=view, dragHandler=this}, 0);

      return true;
    }

    public bool OnDrag(View view, DragEvent dragEvent)
    {
      if (dragEvent.GetLocalState() is MoveDragData)
      {
        if (((MoveDragData)dragEvent.GetLocalState()).dragHandler != this)
        {
          // Not this object
          return false;
        }
      }
      else
      {
        if (Successor is View.IOnDragListener)
        {
          return (Successor as View.IOnDragListener).OnDrag(view, dragEvent);
        }
      }

      bool result = true;

      switch (dragEvent.GetAction())
      {
        case DragEvent.ACTION_DRAG_STARTED:
          view.SetAlpha(0.3f);
          //view.SetVisibility(View.INVISIBLE);
          inDrag = true;
          break;
        case DragEvent.ACTION_DRAG_ENDED:
          view.SetAlpha(1.00f);
          //view.SetVisibility(View.VISIBLE);
          inDrag = false;
          break;
        case DragEvent.ACTION_DRAG_EXITED:
          inDrag = false;
          break;
        case DragEvent.ACTION_DROP:
          break;
        case DragEvent.ACTION_DRAG_LOCATION:
          break;
        default:
          break;
      }

      return result;
    }

  }
}
