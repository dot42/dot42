using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.View;
using SorterenMaar.Palette;
using Android.Widget;
using Android.Content;
using Android.Graphics;
using SorterenMaar.Checkers;
using Android.Graphics.Drawable;
using SorterenMaar.UserControls;
using System.Xml.Linq;
using System.IO;

namespace SorterenMaar.Games
{
  public class SortGameObjects
  {
    public List<View> SortObjects { get; set; }
    public List<View> SortContainers { get; set; }
    public SimpleTextChecker ResultChecker { get; set; }
    public string TaskText { get; set; }
  }


  public class SortTaskFactory
  {
    String strXml = @"  <Task type=""Sort"">
    <DragObjects>
      <Shape shapeType=""Rectangle""  width=""100""  height=""100"" color=""Blue"" helpText=""Vierkant"">
        <TextCheck text=""1""/>
      </Shape>
      <Shape shapeType=""Rectangle"" width=""100"" height=""100"" color=""Green"" helpText=""Vierkant"">
      </Shape>
      <Shape shapeType=""Rectangle"" width=""100"" height=""100"" color=""Red"" helpText=""Vierkant"">
        <TextCheck text=""1""/>
      </Shape>
      <Shape shapeType=""Circle"" width=""100"" height=""100"" color=""Green"" helpText=""Circel"">
        <TextCheck text=""2""/>
      </Shape>
      <Shape shapeType=""Circle"" width=""100"" height=""100"" color=""Red"" helpText=""Circel"">
        <TextCheck text=""2""/>
      </Shape>
      <Shape shapeType=""Circle"" width=""100"" height=""100"" color=""Orange"" helpText=""Circel"">
        <TextCheck text=""2""/>
      </Shape>
      <Shape shapeType=""Triangle"" width=""100"" height=""100"" color=""Red"" helpText=""Driehoek"">
        <TextCheck text=""3""/>
      </Shape>
    </DragObjects>
    <DropObjects>
      <Container background=""Yellow"" orientation=""Horizontal""/>
      <Container background=""Yellow"" orientation=""Horizontal""/>
      <Container background=""Yellow"" orientation=""Horizontal""/>
    </DropObjects>
    <Checker type=""SimpleText""/>
  </Task>";
    string resource;
    SortGameObjects sortObjects;
    ActionModeHandler actionModeHandler;

    public SortTaskFactory(string resource, ActionModeHandler actionModeHandler)
    {
      this.resource = resource;
      this.actionModeHandler = actionModeHandler;
    }

    public SortGameObjects GetNextGameObjects(Context context, Text2Speech tts)
    {
      sortObjects = new SortGameObjects();
      sortObjects.ResultChecker = new SimpleTextChecker();
      sortObjects.SortObjects = CreateSortObjects(context, tts);
      sortObjects.SortContainers = CreateSortContainers(context, tts);
      sortObjects.TaskText = "Wie woont bij elkaar in huis?";
      return sortObjects;

    }

    private List<View> CreateSortObjects(Context context, Text2Speech tts)
    {
      var result = new List<View>();

      var x = ParseXml(context);
      //var p = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WRAP_CONTENT, LinearLayout.LayoutParams.WRAP_CONTENT);
      //p.SetMargins(5, 5, 5, 5);

      //for (int i = 1; i < 15; ++i)
      //{
      //  var s = new TextView(context);
      //  s.SetText("View " + i.ToString());
      //  s.SetLayoutParams(p);
      //  result.Add(s);
      //}

      var s = new ShapeView(context, ShapeView.ShapeEnum.RectangleShape, 100, 100, Color.DKGRAY);
      //var d = new DragHandler { CheckerData = sortObjects.ResultChecker.CreateCheckerData("1") };
      //s.SetOnDragListener(d);
      //s.SetOnLongClickListener(d);
      //s.SetOnTouchListener(d);
      //s.SetLayoutParams(p);
      //result.Add(s);

      //s = new ShapeView(context, ShapeView.ShapeEnum.OvalShape, 100, 75, Color.WHITE);
      //d = new DragHandler { CheckerData = sortObjects.ResultChecker.CreateCheckerData("2") };
      //s.SetOnDragListener(d);
      //s.SetOnLongClickListener(d);
      //s.SetOnTouchListener(d);
      //s.SetLayoutParams(p);
      //result.Add(s);

      //s = new ShapeView(context, ShapeView.ShapeEnum.TriangleShape, 100, 100, Color.MAGENTA);
      //d = new DragHandler { CheckerData = sortObjects.ResultChecker.CreateCheckerData("1") };
      //s.SetOnDragListener(d);
      //s.SetOnLongClickListener(d);
      //s.SetOnTouchListener(d);
      //s.SetLayoutParams(p);
      //result.Add(s);

      //s = new ShapeView(context, ShapeView.ShapeEnum.RoundedRectShape, 100, 100, Color.YELLOW);
      //d = new DragHandler { CheckerData = sortObjects.ResultChecker.CreateCheckerData("2") };
      //s.SetOnDragListener(d);
      //s.SetOnLongClickListener(d);
      //s.SetOnTouchListener(d);
      //s.SetLayoutParams(p);
      //result.Add(s);

      var p = new LinearLayout.LayoutParams(120, 120);
      p.SetMargins(5, 5, 5, 5);

      var i = new ImageView(context);
      i.SetScaleType(Android.Widget.ImageView.ScaleType.CENTER_INSIDE);
      i.SetImageResource(R.Drawables.Laura);
      //- i.SetImageDrawable(Drawable.CreateFromPath("res/drawable/laura.png"));
      i.SetLayoutParams(p);
      var d = new MoveDragHandler { CheckerData = sortObjects.ResultChecker.CreateCheckerData("1") };
      var t = new SpeechDropHandler { Tts = tts, Text = "This is Laura" };
      d.Successor = t;
      i.SetOnDragListener(d);
      i.SetOnTouchListener(d);
      result.Add(i);

      i = new ImageView(context);
      i.SetScaleType(Android.Widget.ImageView.ScaleType.CENTER_INSIDE);
      i.SetImageResource(R.Drawables.Inge);
      //- i.SetImageDrawable(Drawable.CreateFromPath("res/drawable/laura.png"));
      i.SetLayoutParams(p);
      d = new MoveDragHandler { CheckerData = sortObjects.ResultChecker.CreateCheckerData("1") };
      t = new SpeechDropHandler { Tts = tts, Text = "This is Inge" };
      d.Successor = t;
      i.SetOnDragListener(d);
      i.SetOnTouchListener(d);
      result.Add(i);

      i = new ImageView(context);
      i.SetScaleType(Android.Widget.ImageView.ScaleType.CENTER_INSIDE);
      i.SetImageResource(R.Drawables.Emma);
      //- i.SetImageDrawable(Drawable.CreateFromPath("res/drawable/laura.png"));
      i.SetLayoutParams(p);
      d = new MoveDragHandler { CheckerData = sortObjects.ResultChecker.CreateCheckerData("2") };
      t = new SpeechDropHandler { Tts = tts, Text = "This is Emma" };
      d.Successor = t;
      i.SetOnDragListener(d);
      i.SetOnTouchListener(d);
      result.Add(i);

      i = new ImageView(context);
      i.SetScaleType(Android.Widget.ImageView.ScaleType.CENTER_INSIDE);
      i.SetImageResource(R.Drawables.Thor);
      //- i.SetImageDrawable(Drawable.CreateFromPath("res/drawable/laura.png"));
      i.SetLayoutParams(p);
      d = new MoveDragHandler { CheckerData = sortObjects.ResultChecker.CreateCheckerData("2") };
      t = new SpeechDropHandler { Tts = tts, Text = "This is Thor" };
      d.Successor = t;
      i.SetOnDragListener(d);
      i.SetOnTouchListener(d);
      result.Add(i);

      sortObjects.ResultChecker.NrAccepts = result.Count();

      return result;
    }

    private List<View> CreateSortContainers(Context context, Text2Speech tts)
    {
      var result = new List<View>();

      var l = new HorizontalFlowLayout(context);
      l.SetBackgroundColor(context.GetResources().GetColor(R.Colors.light_blue));
      var p = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT, 1);
      //var p = new RelativeLayout.MarginLayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
      p.SetMargins(7, 7, 7, 0);
      l.SetLayoutParams(p);
      var d = new MoveDropHandler { Id = "1" };
      d.OnMoveDropAccepted += sortObjects.ResultChecker.DropHandler;
      l.SetOnDragListener(d);
      result.Add(l);

      l = new HorizontalFlowLayout(context);
      l.SetBackgroundColor(context.GetResources().GetColor(R.Colors.light_blue));
      p = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT, 1);
      //p = new RelativeLayout.MarginLayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
      p.SetMargins(7, 7, 7, 7);
      l.SetLayoutParams(p);
      d = new MoveDropHandler { Id = "2" };
      d.OnMoveDropAccepted += sortObjects.ResultChecker.DropHandler;
      l.SetOnDragListener(d);
      result.Add(l);
      return result;
    }


    private List<ShapeView> ParseXml(Context context)
    {

      var s = new ShapeView(context, ShapeView.ShapeEnum.RectangleShape, 100, 100, Color.DKGRAY);
      //var d = new DragHandler { CheckerData = sortObjects.ResultChecker.CreateCheckerData("1") };
      //s.SetOnDragListener(d);
      //s.SetOnLongClickListener(d);
      //s.SetOnTouchListener(d);
      //s.SetLayoutParams(p);
      //result.Add(s);

      //List<ShapeView> shapeList = new List<ShapeView>();

      List<ShapeView> shapeList =
                      (
                          from e in XDocument.Parse(strXml).Root.Elements("Shape")
                          select new ShapeView(context)
                          {
                            Shape = (ShapeView.ShapeEnum)Enum.Parse(typeof(ShapeView.ShapeEnum), (string)e.Element("shapeType").ToString()),
                            Color = int.Parse(e.Element("color").ToString()),
                            ShapeWidth = int.Parse(e.Element("width").ToString()),
                            ShapeHeight = int.Parse(e.Element("heigth").ToString())
                            //EmployeeID = (int)e.Element("id"),
                            //EmployeeName = (string)e.Element("name"),
                            //EmployeePosition = (string)e.Element("position"),
                            //EmployeeCountry = (string)e.Element("country"),
                            //Projects =
                            //(
                            //    from p in e.Elements("projects").Elements("project")
                            //    select new Project
                            //    {
                            //      ProjectCode = (string)p.Element("code"),
                            //      ProjectBudget = (int)p.Element("budget")
                            //    }).ToArray()
                          }).ToList();


      return shapeList;
    }
  }
}
