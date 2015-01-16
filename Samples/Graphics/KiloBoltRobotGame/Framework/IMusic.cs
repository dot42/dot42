namespace KiloBoltRobotGame.Framework
{
    public interface IMusic
    {
        void play();

        void stop();

        void pause();

        void setLooping(bool looping);

        void setVolume(float volume);

        bool isPlaying();

        bool isStopped();

        bool isLooping();

        void dispose();

        void seekBegin();
    }
}