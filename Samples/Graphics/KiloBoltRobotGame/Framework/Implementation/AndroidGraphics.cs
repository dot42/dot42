using System;
using System.IO;
using Android.Content.Res;
using Android.Graphics;
using Java.Io;

namespace KiloBoltRobotGame.Framework
{
    public class AndroidGraphics : IGraphics
    {
        private readonly AssetManager assets;
        private readonly string assetsPrefix;
        private readonly Bitmap frameBuffer;
        private readonly Canvas canvas;
        private readonly Paint paint;
        private readonly Rect srcRect = new Rect();
        private readonly Rect dstRect = new Rect();

        public AndroidGraphics(AssetManager assets, string assetsPrefix, Bitmap frameBuffer)
        {
            this.assets = assets;
            this.assetsPrefix = assetsPrefix;
            this.frameBuffer = frameBuffer;
            this.canvas = new Canvas(frameBuffer);
            this.paint = new Paint();
        }

        public IImage newImage(string fileName, ImageFormat format)
        {
            Bitmap.Config config = null;
            if (format == ImageFormat.RGB565)
                config = Bitmap.Config.RGB_565;
            else if (format == ImageFormat.ARGB4444)
                config = Bitmap.Config.ARGB_4444;
            else
                config = Bitmap.Config.ARGB_8888;

            var options = new BitmapFactory.Options();
            options.InPreferredConfig = config;


            InputStream @in = null;
            Bitmap bitmap;
            try
            {
                @in = assets.Open(assetsPrefix + fileName);
                bitmap = BitmapFactory.DecodeStream(@in, null, options);
                if (bitmap == null)
                    throw new SystemException("Couldn't load bitmap from asset '" + fileName + "'");
            }
            catch (IOException e)
            {
                throw new SystemException("Couldn't load bitmap from asset '" + fileName + "'");
            }
            finally
            {
                if (@in != null)
                {
                    try
                    {
                        @in.Close();
                    }
                    catch (IOException e)
                    {
                    }
                }
            }

            if (bitmap.GetConfig() == Bitmap.Config.RGB_565)
                format = ImageFormat.RGB565;
            else if (bitmap.GetConfig() == Bitmap.Config.ARGB_4444)
                format = ImageFormat.ARGB4444;
            else
                format = ImageFormat.ARGB8888;

            return new AndroidImage(bitmap, format);
        }

        public void clearScreen(int color)
        {
            canvas.DrawRGB((color & 0xff0000) >> 16, (color & 0xff00) >> 8,
                           (color & 0xff));
        }


        public void drawLine(int x, int y, int x2, int y2, int color)
        {
            paint.SetColor(color);
            canvas.DrawLine(x, y, x2, y2, paint);
        }

        public void drawRect(int x, int y, int width, int height, int color)
        {
            paint.SetColor(color);
            paint.SetStyle(Paint.Style.FILL);
            canvas.DrawRect(x, y, x + width - 1, y + height - 1, paint);
        }

        public void drawARGB(int a, int r, int g, int b)
        {
            paint.SetStyle(Paint.Style.FILL);
            canvas.DrawARGB(a, r, g, b);
        }

        public void drawString(String text, int x, int y, Paint paint)
        {
            canvas.DrawText(text, x, y, paint);
        }


        public void drawImage(IImage Image, int x, int y, int srcX, int srcY,
                              int srcWidth, int srcHeight)
        {
            srcRect.Left = srcX;
            srcRect.Top = srcY;
            srcRect.Right = srcX + srcWidth;
            srcRect.Bottom = srcY + srcHeight;


            dstRect.Left = x;
            dstRect.Top = y;
            dstRect.Right = x + srcWidth;
            dstRect.Bottom = y + srcHeight;

            canvas.DrawBitmap(((AndroidImage) Image).bitmap, srcRect, dstRect, null);
        }

        public void drawImage(IImage Image, int x, int y)
        {
            canvas.DrawBitmap(((AndroidImage) Image).bitmap, x, y, null);
        }

        public void drawScaledImage(IImage Image, int x, int y, int width, int height, int srcX, int srcY, int srcWidth,
                                    int srcHeight)
        {
            srcRect.Left = srcX;
            srcRect.Top = srcY;
            srcRect.Right = srcX + srcWidth;
            srcRect.Bottom = srcY + srcHeight;


            dstRect.Left = x;
            dstRect.Top = y;
            dstRect.Right = x + width;
            dstRect.Bottom = y + height;



            canvas.DrawBitmap(((AndroidImage) Image).bitmap, srcRect, dstRect, null);

        }

        public int getWidth()
        {
            return frameBuffer.GetWidth();
        }

        public int getHeight()
        {
            return frameBuffer.GetHeight();
        }
    }
}