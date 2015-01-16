namespace KiloBoltRobotGame.Framework
{
    public interface IImage
    {
        int getWidth();
        int getHeight();
        ImageFormat getFormat();
        void dispose();
    }
}