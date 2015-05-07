using System;
using Android.Content;using Android.Views;

namespace SorterenMaar.Palette
{
  public class SpeechDragData : IDragData
  {
    public View draggedView;
    public SpeechDragHandler dragHandler;
  }

  public class SpeechDragHandler : DragDropActionHandler, View.IOnDragListener, View.IOnTouchListener
  {
    bool inDrag = false;


    public bool OnTouch(View view, MotionEvent ev)
    {
      //if (inDrag == false)
      //{
        inDrag = true;
        ClipData data = ClipData.NewPlainText("SpeechDragData", "");
        view.StartDrag(data,
                       new Android.Views.View.DragShadowBuilder(view),
                       (Object)new SpeechDragData { draggedView = view, dragHandler = this }, 0);
      //}
      return true;
    }

    public bool OnDrag(View view, DragEvent dragEvent)
    {
      if (dragEvent.LocalState is SpeechDragData)
      {
        if (((SpeechDragData)dragEvent.LocalState).dragHandler != this)
        {
          // Not this object
          return false;
        }
      }
      else
      {
        if (Successor != null)
        {
          if (Successor is View.IOnDragListener)
          {
            return (Successor as View.IOnDragListener).OnDrag(view, dragEvent);
          }
        }
      }

      bool result = true;

      switch (dragEvent.Action)
      {
        case DragEvent.ACTION_DRAG_STARTED:
          //view.Alpha = (0.3f);
          inDrag = true;
          break;
        case DragEvent.ACTION_DRAG_ENDED:
          //view.Alpha = (1.00f);
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
