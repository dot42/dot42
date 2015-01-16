using System;
using System.IO;
using Android.App;
using Android.Content.Res;
using Android.Media;

namespace KiloBoltRobotGame.Framework
{
    public class AndroidAudio : IAudio
    {
        private AssetManager assets;
        private SoundPool soundPool;

        public AndroidAudio(Activity activity)
        {
            activity.SetVolumeControlStream(AudioManager.STREAM_MUSIC);
            this.assets = activity.GetAssets();
            this.soundPool = new SoundPool(20, AudioManager.STREAM_MUSIC, 0);
        }

        public IMusic createMusic(string filename)
        {
            try
            {
                AssetFileDescriptor assetDescriptor = assets.OpenFd(filename);
                return new AndroidMusic(assetDescriptor);
            }
            catch (IOException e)
            {
                throw new SystemException("Couldn't load music '" + filename + "'");
            }
        }

        public ISound createSound(string filename)
        {
            try
            {
                AssetFileDescriptor assetDescriptor = assets.OpenFd(filename);
                int soundId = soundPool.Load(assetDescriptor, 0);
                return new AndroidSound(soundPool, soundId);
            }
            catch (IOException e)
            {
                throw new SystemException("Couldn't load sound '" + filename + "'");
            }
        }
    }
}