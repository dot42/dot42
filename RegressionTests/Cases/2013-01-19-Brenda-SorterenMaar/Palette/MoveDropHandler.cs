using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;using Android.Views;
using Android.Content;
using Android.Graphics;
using Android.Animation;
using Android.Views.Animations;
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
      if (dragEvent.LocalState is MoveDragData)
      {
        dragData = dragEvent.LocalState as MoveDragData;
      }
      else
      {
        if (Successor is View.IOnDragListener)
        {
          return (Successor as View.IOnDragListener).OnDrag(view, dragEvent);
        }
      }

      switch (dragEvent.Action)
      {
        case DragEvent.ACTION_DRAG_STARTED:
          break;
        case DragEvent.ACTION_DRAG_ENTERED:

          view.SetBackgroundColor(view.Context.Resources.GetColor(R.Color.accent_blue));
          //float[] single = { 1.0F, 0.5F };
          //anim = ObjectAnimator.OfFloat((Object)view, "alpha", single);
          //anim.SetInterpolator(new CycleInterpolator(40));
          //anim.SetDuration(30 * 1000); // 30 seconds
          //anim.Start();
          break;
        case DragEvent.ACTION_DRAG_ENDED:
        case DragEvent.ACTION_DRAG_EXITED:
          view.SetBackgroundColor(view.Context.Resources.GetColor(R.Color.light_blue));
          //if (anim != null)
          //{
          //  anim.End();
          //  anim = null;
          //}
          break;
        case DragEvent.ACTION_DROP:
          view.SetBackgroundColor(view.Context.Resources.GetColor(R.Color.light_blue));
          //if (anim != null)
          //{
          //  anim.End();
          //  anim = null;
          //}

          // Dropped, reassign View to ViewGroup
          var dragedView = dragData.draggedView;
          ViewGroup owner = (ViewGroup)dragedView.Parent;
          owner.RemoveView(dragedView);
          //LinearLayout container = (LinearLayout)view;
          HorizontalFlowLayout container = (HorizontalFlowLayout)view;
          container.AddView(dragedView);
          dragedView.Visibility = (View.VISIBLE);

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
