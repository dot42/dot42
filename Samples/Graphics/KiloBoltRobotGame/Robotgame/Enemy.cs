using System;
using Android.Graphics;

namespace KiloBoltRobotGame.Robotgame
{
    public class Enemy
    {

        private int power, centerX, speedX, centerY;
        private Background bg = GameScreen.getBg1();
        private Robot robot = GameScreen.getRobot();

        public Rect r = new Rect(0, 0, 0, 0);
        public int health = 5;

        private int movementSpeed;

        // Behavioral Methods
        public void update()
        {
            follow();
            centerX += speedX;
            speedX = bg.getSpeedX()*5 + movementSpeed;
            r.Set(centerX - 25, centerY - 25, centerX + 25, centerY + 35);

            if (Rect.Intersects(r, Robot.yellowRed))
            {
                checkCollision();
            }


        }

        private void checkCollision()
        {
            if (Rect.Intersects(r, Robot.rect) || Rect.Intersects(r, Robot.rect2)
                || Rect.Intersects(r, Robot.rect3) || Rect.Intersects(r, Robot.rect4))
            {

            }
        }

        public void follow()
        {

            if (centerX < -95 || centerX > 810)
            {
                movementSpeed = 0;
            }

            else if (Math.Abs(robot.getCenterX() - centerX) < 5)
            {
                movementSpeed = 0;
            }

            else
            {

                if (robot.getCenterX() >= centerX)
                {
                    movementSpeed = 1;
                }
                else
                {
                    movementSpeed = -1;
                }
            }

        }

        public void die()
        {

        }

        public void attack()
        {

        }

        public int getPower()
        {
            return power;
        }

        public int getSpeedX()
        {
            return speedX;
        }

        public int getCenterX()
        {
            return centerX;
        }

        public int getCenterY()
        {
            return centerY;
        }

        public Background getBg()
        {
            return bg;
        }

        public void setPower(int power)
        {
            this.power = power;
        }

        public void setSpeedX(int speedX)
        {
            this.speedX = speedX;
        }

        public void setCenterX(int centerX)
        {
            this.centerX = centerX;
        }

        public void setCenterY(int centerY)
        {
            this.centerY = centerY;
        }

        public void setBg(Background bg)
        {
            this.bg = bg;
        }

    }
}