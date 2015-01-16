namespace KiloBoltRobotGame.Framework
{
    public interface IGame
    {

        IAudio getAudio();

        IInput getInput();

        IFileIO getFileIO();

        IGraphics getGraphics();

        void setScreen(Screen screen);

        Screen getCurrentScreen();

        Screen getInitScreen();
    }    
}