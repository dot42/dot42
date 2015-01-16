using System.IO;
using System.Text;
using Android.Util;
using Dot42.Manifest;
using Java.Io;
using KiloBoltRobotGame.Framework;

namespace KiloBoltRobotGame.Robotgame
{
    [Activity(Label = "RobotGame", ScreenOrientation = ScreenOrientations.Landscape, ConfigChanges = ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.Orientation)]
    public class SampleGame : AndroidGame
    {

        public static string map;
        private bool firstTimeCreate = true;

        public override Screen getInitScreen()
        {

            if (firstTimeCreate)
            {
                Robotgame.Assets.load(this);
                firstTimeCreate = false;
            }

            InputStream @is = GetResources().OpenRawResource(R.Raws.map1);
            map = convertStreamToString(@is);

            return new SplashLoadingScreen(this);
        }

        public override string GetAssetsPrefix()
        {
            return Robotgame.Assets.Prefix;
        }

        public void onBackPressed()
        {
            getCurrentScreen().backButton();
        }

        private static string convertStreamToString(InputStream @is)
        {

            BufferedReader reader = new BufferedReader(new InputStreamReader(@is));
            StringBuilder sb = new StringBuilder();

            string line = null;
            try
            {
                while ((line = reader.ReadLine()) != null)
                {
                    sb.Append((line + "\n"));
                }
            }
            catch (IOException e)
            {
                Log.W("LOG", e.GetMessage());
            }
            finally
            {
                try
                {
                    @is.Close();
                }
                catch (IOException e)
                {
                    Log.W("LOG", e.GetMessage());
                }
            }
            return sb.ToString();
        }

        protected override void OnResume()
        {
            base.OnResume();
            Robotgame.Assets.theme.play();
        }

        protected override void OnPause()
        {
            base.OnPause();
            Robotgame.Assets.theme.pause();
        }
    }
}