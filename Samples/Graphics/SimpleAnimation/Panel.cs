using System.Threading;
using Android.Content;
using Android.Graphics;
using Android.View;
using Java.Lang;

namespace SimpleAnimation
{
    internal class Panel : SurfaceView, ISurfaceHolder_ICallback, IRunnable
    {
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

        private void DoDraw(Canvas canvas)
        {
			// Increment / reset
            size += delta;
            if (size > 250)
            {
				delta = -1;
            } 
			else if (size < 30) 
			{
				delta = 1;
			}

            // Set background color
            canvas.DrawColor(Color.BLUE); 
            var paint = new Paint();
            paint.SetTextAlign(Paint.Align.CENTER);

            // Draw some lines
            canvas.DrawLine(mX, mY, mY + 33, mX + 100, paint);
            paint.SetColor(Color.RED);
            paint.SetStrokeWidth(10);
            canvas.DrawLine(87, 0, 75, 100, paint);
            paint.SetColor(Color.GREEN);
            paint.SetStrokeWidth(5);
            for (int y = 30, alpha = 128; alpha > 2; alpha >>= 1, y += 10)
            {
                paint.SetAlpha(alpha);

                canvas.DrawLine(mY, y, mY + 100, y, paint);
            }

            // Draw a red rectangle
            paint.SetColor(Color.RED);
            var rect = new Rect();
            rect.Set(size + 120, 130, size + 156, 156);
            canvas.DrawRect(rect, paint);

            // Draw a circle
            paint.SetColor(Color.ParseColor("#ffd700"));
            canvas.DrawCircle(size * 2, 220, 30, paint); //faster circle

            // Draw red'ish rectangle
            paint.SetColor(Color.Rgb(128, 20, 20));
            canvas.DrawRect(size, 67, 68, 45, paint);

            // Draw green circle
            paint.SetColor(Color.GREEN);
            canvas.DrawCircle(size, 140.0f - size / 3, 45.0f, paint); //move circle across screen
            paint.SetColor(Color.RED);
            canvas.DrawText("Dot42", size, 140.0f - size / 3, paint);

            // Draw magenta circle
            paint.SetColor(Color.MAGENTA);
            canvas.DrawCircle(mX, mY, size / 4.0f, paint); //move circle down screen
            paint.SetColor(Color.GREEN);
            canvas.DrawText("is", mX, mY, paint);

            // Draw yellow rectangle
            paint.SetAlpha(64);
            paint.SetColor(Color.YELLOW);
            canvas.DrawRect(size, size, size + 45, size + 45, paint);
            // Draw text on rectangle
            paint.SetAlpha(255);
            paint.SetColor(Color.DKGRAY);
            canvas.DrawText("fun!", size + 45 / 2, size + 45 / 2, paint);
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
            while (running)
            {
                var canvas = holder.LockCanvas();
                if (canvas != null)
                {
                    DoDraw(canvas);
                    holder.UnlockCanvasAndPost(canvas);
					Thread.Sleep(5);
                }
            }
        }
    }
}
