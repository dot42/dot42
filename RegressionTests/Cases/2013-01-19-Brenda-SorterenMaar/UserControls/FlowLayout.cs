using System;
using Android.Content;
using Android.Util;using Android.Views;
using Android.Widget;

namespace SorterenMaar.UserControls
{
  // http://adilatwork.blogspot.nl/2012/12/android-horizontal-flow-layout.html
  /** Custom view which extends {@link RelativeLayout}
   * and which places its children horizontally,
   * flowing over to a new line whenever it runs out of width.*/
  public class HorizontalFlowLayout : RelativeLayout
  {
    /** Constructor to use when creating View from code.*/
    public HorizontalFlowLayout(Context context)
      : base(context)
    {
    }

    /** Constructor that is called when inflating View from XML.*/
    public HorizontalFlowLayout(Context context, IAttributeSet attrs)
      : base(context, attrs)
    {
    }

    /** Perform inflation from XML and apply a class-specific base style.*/
    public HorizontalFlowLayout(Context context, IAttributeSet attrs, int defStyle)
      : base(context, attrs, defStyle)
    {
    }

    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
      // need to call base.OnMeasure(...) otherwise Get some funny behaviour
      base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

      int width = MeasureSpec.GetSize(widthMeasureSpec);
      int height = MeasureSpec.GetSize(heightMeasureSpec);

      // increment the x position as we progress through a line
      int xpos = PaddingLeft;
      // increment the y position as we progress through the lines
      int ypos = PaddingTop;
      // the height of the current line
      int line_height = 0;

      // go through children
      // to work out the height required for this view

      // call to measure size of children not needed I think?!
      // Getting child's measured height/width seems to work okay without it
      //measureChildren(widthMeasureSpec, heightMeasureSpec);

      View child;
      MarginLayoutParams childMarginLayoutParams;
      int childWidth, childHeight, childMarginLeft, childMarginRight, childMarginTop, childMarginBottom;

      for (int i = 0; i < ChildCount; i++)
      {
        child = GetChildAt(i);

        if (child.Visibility!= GONE)
        {
          childWidth = child.MeasuredWidth;
          childHeight = child.MeasuredHeight;

          if (child.LayoutParameters != null
              && child.LayoutParameters is MarginLayoutParams)
          {
            childMarginLayoutParams = (MarginLayoutParams)child.LayoutParameters;

            childMarginLeft = childMarginLayoutParams.LeftMargin;
            childMarginRight = childMarginLayoutParams.RightMargin;
            childMarginTop = childMarginLayoutParams.TopMargin;
            childMarginBottom = childMarginLayoutParams.BottomMargin;
          }
          else
          {
            childMarginLeft = 0;
            childMarginRight = 0;
            childMarginTop = 0;
            childMarginBottom = 0;
          }

          if (xpos + childMarginLeft + childWidth + childMarginRight + PaddingRight > width)
          {
            // this child will need to go on a new line

            xpos = PaddingLeft;
            ypos += line_height;

            line_height = childMarginTop + childHeight + childMarginBottom;
          }
          else
            // enough space for this child on the current line
            line_height = Math.Max(
                line_height,
                childMarginTop + childHeight + childMarginBottom);

          xpos += childMarginLeft + childWidth + childMarginRight;
        }
      }

      ypos += line_height + PaddingBottom;

      if (MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpec.UNSPECIFIED)
        // set height as measured since there's no height restrictions
        height = ypos;
      else if (MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpec.AT_MOST
          && ypos < height)
        // set height as measured since it's less than the maximum allowed
        height = ypos;

      SetMeasuredDimension(width, height);
    }

    protected override void OnLayout(bool changed, int l, int t, int r, int b)
    {
      // increment the x position as we progress through a line
      int xpos = PaddingLeft;
      // increment the y position as we progress through the lines
      int ypos = PaddingTop;
      // the height of the current line
      int line_height = 0;

      View child;
      MarginLayoutParams childMarginLayoutParams;
      int childWidth, childHeight, childMarginLeft, childMarginRight, childMarginTop, childMarginBottom;

      for (int i = 0; i < ChildCount; i++)
      {
        child = GetChildAt(i);

        if (child.Visibility != GONE)
        {
          childWidth = child.MeasuredWidth;
          childHeight = child.MeasuredHeight;

          if (child.LayoutParameters != null
              && child.LayoutParameters is MarginLayoutParams)
          {
            childMarginLayoutParams = (MarginLayoutParams)child.LayoutParameters;

            childMarginLeft = childMarginLayoutParams.LeftMargin;
            childMarginRight = childMarginLayoutParams.RightMargin;
            childMarginTop = childMarginLayoutParams.TopMargin;
            childMarginBottom = childMarginLayoutParams.BottomMargin;
          }
          else
          {
            childMarginLeft = 0;
            childMarginRight = 0;
            childMarginTop = 0;
            childMarginBottom = 0;
          }

          if (xpos + childMarginLeft + childWidth + childMarginRight + PaddingRight > r - l)
          {
            // this child will need to go on a new line

            xpos = PaddingLeft;
            ypos += line_height;

            line_height = childHeight + childMarginTop + childMarginBottom;
          }
          else
            // enough space for this child on the current line
            line_height = Math.Max(
                line_height,
                childMarginTop + childHeight + childMarginBottom);

          child.Layout(
              xpos + childMarginLeft,
              ypos + childMarginTop,
              xpos + childMarginLeft + childWidth,
              ypos + childMarginTop + childHeight);

          xpos += childMarginLeft + childWidth + childMarginRight;
        }
      }
    }
  }
}
