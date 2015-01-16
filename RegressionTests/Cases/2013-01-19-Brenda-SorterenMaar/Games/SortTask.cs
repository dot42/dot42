using System;
using Android.View;
using Android.Widget;
using SorterenMaar.Games;
using SorterenMaar.Palette;

namespace SorterenMaar
{

  public class SortTask : ITask
  {
    int layoutId = R.Layouts.SortTaskLayout;
    SortTaskFactory factory;
    Text2Speech tts = null;

    View v;

    public SortTask(string resource, ActionModeHandler actionModeHandler)
    {
      factory = new SortTaskFactory(resource, actionModeHandler);
    }

    View ITask.CreateView(LayoutInflater inflater, ViewGroup container)
    {
      v = inflater.Inflate(layoutId, container, false);

      tts = new Text2Speech(v.GetContext());
      // Create all parts from, for instance, a xml file
      var s = factory.GetNextGameObjects(v.GetContext(), tts);
      CreateTask(v, s);

      return v;
    }


    private void CreateTask(View v, SortGameObjects s)
    {
      s.ResultChecker.OnTaskFinished += TaskFinishedHandler;
      var v_tasktext = v.FindViewById<TextView>(R.Ids.TaskText);
      if (v_tasktext == null)
      {
        throw new NullReferenceException("No task text view");
      }
      var v_earImage = v.FindViewById<ImageView>(R.Ids.earImage);
      if (v_earImage == null)
      {
        throw new NullReferenceException("No ear image view");
      }

      var v_task = v.FindViewById<ViewGroup>(R.Ids.Task);
      if (v_task == null)
      {
        throw new NullReferenceException("No container for sort objects");
      }

      var v_dropzones = v.FindViewById<ViewGroup>(R.Ids.DropZones);
      if (v_dropzones == null)
      {
        throw new NullReferenceException("No container for sort containers");
      }

      v_dropzones.RemoveAllViews();

      v_tasktext.SetText(s.TaskText);
      var h = new SpeechDragHandler();
      v_earImage.SetOnDragListener(h);
      v_earImage.SetOnTouchListener(h);
      foreach (var o in s.SortObjects)
      {
        v_task.AddView(o);
      }
      foreach (var o in s.SortContainers)
      {
        v_dropzones.AddView(o);
      }

      //(s.SortContainers[0] as HorizontalFlowLayout).AddView(s.SortObjects[0]);
      //s.SortObjects.RemoveAt(0);
      //foreach (var o in s.SortObjects)
      //{
      //  v_task.AddView(o);
      //  //(s.SortContainers[0] as HorizontalFlowLayout).AddView(o);

      //}

    }

    public void TaskFinishedHandler(bool result)
    {
      // Create all parts from, for instance, a xml file
      var s = factory.GetNextGameObjects(v.GetContext(), tts);
      if (s != null)
      {
        CreateTask(v, s);
      }
      else
      {
        // Game finished
      }
    }
  }



}
