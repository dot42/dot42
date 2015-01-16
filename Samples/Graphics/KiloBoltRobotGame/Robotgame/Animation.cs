using Java.Util;
using KiloBoltRobotGame.Framework;

namespace KiloBoltRobotGame.Robotgame
{
    public class Animation
    {

        private ArrayList<AnimFrame> frames;
        private int currentFrame;
        private long animTime;
        private long totalDuration;

        public Animation()
        {
            frames = new ArrayList<AnimFrame>();
            totalDuration = 0;

            lock(this)
            {
                animTime = 0;
                currentFrame = 0;
            }
        }

        public /*synchronized */ void addFrame(IImage image, long duration)
        {
            lock (this)
            {
                totalDuration += duration;
                frames.Add(new AnimFrame(image, totalDuration));
            }
        }

        public /*synchronized*/ void update(long elapsedTime)
        {
            lock (this)
            {
                if (frames.Size() > 1)
                {
                    animTime += elapsedTime;
                    if (animTime >= totalDuration)
                    {
                        animTime = animTime%totalDuration;
                        currentFrame = 0;

                    }

                    while (animTime > getFrame(currentFrame).endTime)
                    {
                        currentFrame++;

                    }
                }
            }
        }

        public /*synchronized*/ IImage getImage()
        {
            lock (this)
            {
                if (frames.Size() == 0)
                {
                    return null;
                }
                else
                {
                    return getFrame(currentFrame).image;
                }
            }
        }

        private AnimFrame getFrame(int i)
        {
            return (AnimFrame) frames.Get(i);
        }

        private class AnimFrame
        {
            internal readonly IImage image;
            internal readonly long endTime;

            public AnimFrame(IImage image, long endTime)
            {
                this.image = image;
                this.endTime = endTime;
            }
        }
    }
}