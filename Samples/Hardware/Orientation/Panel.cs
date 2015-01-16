using System;
using System.Threading;
using Android.Content;
using Android.Graphics;
using Android.View;
using Java.Lang;

namespace Orientation
{
    internal class Panel : SurfaceView, ISurfaceHolder_ICallback, IRunnable
    {
       private double xOrientation = 0;
       private double yOrientation = 0;

        private float mX;
        private float mY;
        private int size = 50;
        private int delta = 1;
        private bool running;
        private readonly ISurfaceHolder holder;

        public Panel(Context context) : base(context)
        {
            holder = GetHolder();
            holder.AddCallback(this);
        }

        public void setOrientation(double x, double y)
        {
           xOrientation = x;
           yOrientation = y;
        }

        private double GetStepFactor(Canvas canvas)
        {
           if (canvas != null)
           {
              double width = canvas.GetWidth();
              double height = canvas.GetHeight();

              double x = width/2;
              double y = height/2;

              double maxDistance = Math.Min(x, y);
              return maxDistance/10; // assumes that 10 is the maximum gravitational value.
           }
           return 0;
        }

       private void DoDraw(Canvas canvas, double stepFactor)
         {
            double width = canvas.GetWidth();
            double height = canvas.GetHeight();

            double x = width/2;
            double y = height/2;

            x += xOrientation * stepFactor;
            y += yOrientation * stepFactor;

            // Set background color
            canvas.DrawColor(Color.BLUE);
            var paint = new Paint();
            paint.SetTextAlign(Paint.Align.CENTER);

            // Draw a circle
            paint.SetColor(Color.ParseColor("#ffd700"));
            canvas.DrawCircle((float) x, (float) y, 30, paint);
         }

       public void SurfaceCreated(ISurfaceHolder iSurfaceHolder)
        {
            var thread = new Thread(this);
            running = true;
            thread.Start();
        }

        public void SurfaceChanged(ISurfaceHolder iSurfaceHolder, int int32, int int321, int int322)
        {
        }

        public void SurfaceDestroyed(ISurfaceHolder iSurfaceHolder)
        {
            running = false;
        }

        public override bool OnTouchEvent(MotionEvent @event)
        {
            mX =  @event.X;
            mY =  @event.Y;
            return true;
        }

        public void Run()
        {
           double oldXOrientation = xOrientation;
           double oldYOrientation = yOrientation;
           double stepFactor = 0;

           var canvas = holder.LockCanvas();
           if (canvas != null)
           {
              stepFactor = GetStepFactor(canvas);
              holder.UnlockCanvasAndPost(canvas);
           }
           while (running)
            {
               // Don't draw too often, 100Hz should be enough.
               Thread.Sleep(10);

               // Only draw if changes are more than a pixel.

               if (Math.Abs(xOrientation - oldXOrientation) * stepFactor > 1 || Math.Abs(yOrientation - oldYOrientation) * stepFactor > 1)
               {
                  // remember where we draw.
                  oldXOrientation = xOrientation;
                  oldYOrientation = yOrientation;

                  canvas = holder.LockCanvas();
                  if (canvas != null)
                  {
                     DoDraw(canvas, stepFactor);
                     holder.UnlockCanvasAndPost(canvas);
                  }
               }

            }
        }
    }
}
