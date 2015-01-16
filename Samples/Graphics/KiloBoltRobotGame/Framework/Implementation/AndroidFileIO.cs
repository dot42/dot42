using Android.Content;
using Android.Content.Res;
using Android.Os;
using Android.Preference;
using Java.Io;

namespace KiloBoltRobotGame.Framework
{
    public class AndroidFileIO : IFileIO
    {
        readonly Context context;
        readonly AssetManager assets;
        readonly string externalStoragePath;

        public AndroidFileIO(Context context)
        {
            this.context = context;
            this.assets = context.GetAssets();
            this.externalStoragePath = Environment.GetExternalStorageDirectory().GetAbsolutePath() + File.Separator;
        }

        public InputStream readAsset(string file)
        {
            return assets.Open(file);
        }

        public InputStream readFile(string file)
        {
            return new FileInputStream(externalStoragePath + file);
        }

        public OutputStream writeFile(string file)
        {
            return new FileOutputStream(externalStoragePath + file);
        }

        public ISharedPreferences getSharedPref()
        {
            return PreferenceManager.GetDefaultSharedPreferences(context);
        }
    }

}