using System.Threading;
using Android.Graphics;
using Android.View;
using Java.Lang;

namespace KiloBoltRobotGame.Framework
{
    public class AndroidFastRenderView : SurfaceView, IRunnable
    {
        private readonly AndroidGame game;
        private readonly Bitmap framebuffer;
        private Thread renderThread = null;
        private readonly ISurfaceHolder holder;
        private /*volatile*/ bool running = false;

        public AndroidFastRenderView(AndroidGame game, Bitmap framebuffer)
            : base(game)
        {
            this.game = game;
            this.framebuffer = framebuffer;
            this.holder = GetHolder();

        }

        public void resume()
        {
            running = true;
            renderThread = new Thread(this);
            renderThread.Start();

        }

        public void Run()
        {
            Rect dstRect = new Rect();
            long startTime = Java.Lang.System.NanoTime();
            while (running)
            {
                if (!holder.GetSurface().IsValid())
                    continue;


                float deltaTime = (Java.Lang.System.NanoTime() - startTime) / 10000000.000f;
                startTime = Java.Lang.System.NanoTime();

                if (deltaTime > 3.15)
                {
                    deltaTime = (float)3.15;
                }


                game.getCurrentScreen().update(deltaTime);
                game.getCurrentScreen().paint(deltaTime);



                Canvas canvas = holder.LockCanvas();
                canvas.GetClipBounds(dstRect);
                canvas.DrawBitmap(framebuffer, null, dstRect, null);
                holder.UnlockCanvasAndPost(canvas);


            }
        }

        public void pause()
        {
            running = false;
            while (true)
            {
                try
                {
                    renderThread.Join();
                    break;
                }
                catch (InterruptedException e)
                {
                    // retry
                }
            }
        }
    }
}