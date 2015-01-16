using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Os;
using Android.View;

namespace KiloBoltRobotGame.Framework
{
    public abstract class AndroidGame : Activity, IGame
    {
        private AndroidFastRenderView renderView;
        private IGraphics graphics;
        private IAudio audio;
        private IInput input;
        private IFileIO fileIO;
        private Screen screen;
        private PowerManager.WakeLock wakeLock;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature(Window.FEATURE_NO_TITLE);
            GetWindow().SetFlags(IWindowManager_LayoutParams.FLAG_FULLSCREEN,
                                 IWindowManager_LayoutParams.FLAG_FULLSCREEN);

            bool isPortrait = GetResources().GetConfiguration().Orientation == Configuration.ORIENTATION_PORTRAIT;
            int frameBufferWidth = isPortrait ? 480 : 800;
            int frameBufferHeight = isPortrait ? 800 : 480;
            Bitmap frameBuffer = Bitmap.CreateBitmap(frameBufferWidth,
                                                     frameBufferHeight, Bitmap.Config.RGB_565);

            float scaleX = (float)frameBufferWidth
                           / GetWindowManager().GetDefaultDisplay().GetWidth();
            float scaleY = (float)frameBufferHeight
                           / GetWindowManager().GetDefaultDisplay().GetHeight();

            renderView = new AndroidFastRenderView(this, frameBuffer);
            graphics = new AndroidGraphics(GetAssets(), GetAssetsPrefix(), frameBuffer);
            fileIO = new AndroidFileIO(this);
            audio = new AndroidAudio(this);
            input = new AndroidInput(this, renderView, scaleX, scaleY);
            screen = getInitScreen();
            SetContentView(renderView);

            PowerManager powerManager = (PowerManager)GetSystemService(Context.POWER_SERVICE);
            wakeLock = powerManager.NewWakeLock(PowerManager.FULL_WAKE_LOCK, "MyGame");
        }

        protected override void OnResume()
        {
            base.OnResume();
            wakeLock.Acquire();
            screen.resume();
            renderView.resume();
        }

        protected override void OnPause()
        {
            base.OnPause();
            wakeLock.Release();
            renderView.pause();
            screen.pause();

            if (IsFinishing())
                screen.dispose();
        }

        public IInput getInput()
        {
            return input;
        }

        public IFileIO getFileIO()
        {
            return fileIO;
        }

        public IGraphics getGraphics()
        {
            return graphics;
        }

        public IAudio getAudio()
        {
            return audio;
        }

        public void setScreen(Screen screen)
        {
            if (screen == null)
                throw new ArgumentException("Screen must not be null");

            this.screen.pause();
            this.screen.dispose();
            screen.resume();
            screen.update(0);
            this.screen = screen;
        }

        public Screen getCurrentScreen()
        {

            return screen;
        }

        public abstract Screen getInitScreen();

        public abstract string GetAssetsPrefix();
    }
}