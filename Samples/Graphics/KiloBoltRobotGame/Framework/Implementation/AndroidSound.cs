using Android.Media;

namespace KiloBoltRobotGame.Framework
{
    public class AndroidSound : ISound
    {
        private readonly int soundId;
        private readonly SoundPool soundPool;

        public AndroidSound(SoundPool soundPool, int soundId)
        {
            this.soundId = soundId;
            this.soundPool = soundPool;
        }

        public void play(float volume)
        {
            soundPool.Play(soundId, volume, volume, 0, 0, 1);
        }

        public void dispose()
        {
            soundPool.Unload(soundId);
        }
    }
}