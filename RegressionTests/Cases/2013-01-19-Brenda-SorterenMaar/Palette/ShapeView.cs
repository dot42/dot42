using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.View;
using Android.Graphics.Drawable;
using Android.Content;
using Android.Graphics.Drawable.Shapes;
using Android.Graphics;
using Android.Widget;

namespace SorterenMaar.Palette
{
  public class ShapeView : View, IObjectData
  {
    
    public enum ShapeEnum
    {
      RectangleShape,
      OvalShape,
      TriangleShape,
      ArcShape,
      RoundedRectShape
    }

    public int ShapeWidth { get; set; }
    public int ShapeHeight { get; set; }
    public int Color { get; set; }
    public ShapeEnum Shape { get; set; }

    public ShapeView(Context context)
      : base(context)
    {
    }

    public ShapeView(Context context, ShapeEnum shape,  int width, int height, int color)
      : base(context)
    {
      ShapeWidth = width;
      ShapeHeight = height;
      Color = color;
      Shape = shape;

    }

    protected void DrawShape(Canvas canvas)
    {
      Paint paint = new Paint();
      paint.Color = Color;
      switch (Shape)
      {
        case ShapeEnum.RectangleShape:
          canvas.DrawRect(0, 0, ShapeWidth, ShapeHeight, paint);
          break;
        case ShapeEnum.OvalShape:
          canvas.DrawOval(new RectF(0, 0, ShapeWidth, ShapeHeight), paint);
          break;
        case ShapeEnum.TriangleShape:
            Path path = new Path();
            path.MoveTo(ShapeWidth / 2, 0);
            path.LineTo(ShapeWidth, ShapeHeight);
            path.LineTo(0,ShapeHeight);
            path.Close();
          canvas.DrawPath(path, paint);
          break;
        default:
          canvas.DrawCircle(ShapeWidth / 2, ShapeHeight / 2, ShapeWidth / 2, paint); 
          break;
      }
    }

    protected override void OnDraw(Canvas canvas)
    {
      DrawShape(canvas);
    }

    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
      SetMeasuredDimension(ShapeWidth, ShapeHeight);
    }

  }

}
