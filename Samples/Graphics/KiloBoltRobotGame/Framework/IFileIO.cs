using Java.Io;
using Android.Content;

namespace KiloBoltRobotGame.Framework
{
    public interface IFileIO
    {
        InputStream readFile(string file);

        OutputStream writeFile(string file);

        InputStream readAsset(string file);

        ISharedPreferences getSharedPref();
    }
}