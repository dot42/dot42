using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.View;
using Android.Content;
using Android.Graphics;
using Android.Animation;
using Android.View.Animation;
using Android.Widget;
using SorterenMaar.Checkers;
using SorterenMaar.Games;
using SorterenMaar.UserControls;

namespace SorterenMaar.Palette
{
  public delegate void MoveDropAccepted(string oldContainerId, string newContainerId, ICheckerData data);

  public class MoveDropHandler : DragDropActionHandler, View.IOnDragListener
  {
    //ObjectAnimator anim;
    public event MoveDropAccepted OnMoveDropAccepted;
    public string Id { get; set; }

    public MoveDropHandler()
    {
    }

    public bool OnDrag(View view, DragEvent dragEvent)
    {
      bool result = true;

      MoveDragData dragData = null;
      if (dragEvent.GetLocalState() is MoveDragData)
      {
        dragData = dragEvent.GetLocalState() as MoveDragData;
      }
      else
      {
        if (Successor is View.IOnDragListener)
        {
          return (Successor as View.IOnDragListener).OnDrag(view, dragEvent);
        }
      }

      switch (dragEvent.GetAction())
      {
        case DragEvent.ACTION_DRAG_STARTED:
          break;
        case DragEvent.ACTION_DRAG_ENTERED:

          view.SetBackgroundColor(view.GetContext().GetResources().GetColor(R.Colors.accent_blue));
          //float[] single = { 1.0F, 0.5F };
          //anim = ObjectAnimator.OfFloat((Object)view, "alpha", single);
          //anim.SetInterpolator(new CycleInterpolator(40));
          //anim.SetDuration(30 * 1000); // 30 seconds
          //anim.Start();
          break;
        case DragEvent.ACTION_DRAG_ENDED:
        case DragEvent.ACTION_DRAG_EXITED:
          view.SetBackgroundColor(view.GetContext().GetResources().GetColor(R.Colors.light_blue));
          //if (anim != null)
          //{
          //  anim.End();
          //  anim = null;
          //}
          break;
        case DragEvent.ACTION_DROP:
          view.SetBackgroundColor(view.GetContext().GetResources().GetColor(R.Colors.light_blue));
          //if (anim != null)
          //{
          //  anim.End();
          //  anim = null;
          //}

          // Dropped, reassign View to ViewGroup
          var dragedView = dragData.draggedView;
          ViewGroup owner = (ViewGroup)dragedView.GetParent();
          owner.RemoveView(dragedView);
          //LinearLayout container = (LinearLayout)view;
          HorizontalFlowLayout container = (HorizontalFlowLayout)view;
          container.AddView(dragedView);
          dragedView.SetVisibility(View.VISIBLE);

          // Inform all listeners
          OnMoveDropAccepted(dragData.dragHandler.CurrentContainer, Id, (dragData as MoveDragData).dragHandler.CheckerData);
          // Set as currentContainer
          dragData.dragHandler.CurrentContainer = Id;

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
