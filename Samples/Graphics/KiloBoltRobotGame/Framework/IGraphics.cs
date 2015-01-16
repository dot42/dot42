using Android.Graphics;

namespace KiloBoltRobotGame.Framework
{
    public interface IGraphics
    {
        IImage newImage(string fileName, ImageFormat format);

        void clearScreen(int color);

        void drawLine(int x, int y, int x2, int y2, int color);

        void drawRect(int x, int y, int width, int height, int color);

        void drawImage(IImage image, int x, int y, int srcX, int srcY,
                       int srcWidth, int srcHeight);

        void drawImage(IImage Image, int x, int y);

        void drawString(string text, int x, int y, Paint paint);

        int getWidth();

        int getHeight();

        void drawARGB(int i, int j, int k, int l);

    }
}