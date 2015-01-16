namespace KiloBoltRobotGame.Framework
{
    public interface IAudio
    {
        IMusic createMusic(string file);

        ISound createSound(string file);
    }

}