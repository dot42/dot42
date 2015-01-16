using Android.Graphics;
using Java.Util;

namespace KiloBoltRobotGame.Robotgame
{
    public class Robot
    {

        // Constants are Here
        private const int JUMPSPEED = -15;
        private const int MOVESPEED = 5;

        private int centerX = 100;
        private int centerY = 377;
        private bool jumped = false;
        private bool movingLeft = false;
        private bool movingRight = false;
        private bool ducked = false;
        private bool readyToFire = true;

        private int speedX = 0;
        private int speedY = 0;
        public static Rect rect = new Rect(0, 0, 0, 0);
        public static Rect rect2 = new Rect(0, 0, 0, 0);
        public static Rect rect3 = new Rect(0, 0, 0, 0);
        public static Rect rect4 = new Rect(0, 0, 0, 0);
        public static Rect yellowRed = new Rect(0, 0, 0, 0);

        public static Rect footleft = new Rect(0, 0, 0, 0);
        public static Rect footright = new Rect(0, 0, 0, 0);


        private Background bg1 = GameScreen.getBg1();
        private Background bg2 = GameScreen.getBg2();

        private ArrayList<Projectile> projectiles = new ArrayList<Projectile>();

        public void update()
        {
            // Moves Character or Scrolls Background accordingly.

            if (speedX < 0)
            {
                centerX += speedX;
            }
            if (speedX == 0 || speedX < 0)
            {
                bg1.setSpeedX(0);
                bg2.setSpeedX(0);

            }
            if (centerX <= 200 && speedX > 0)
            {
                centerX += speedX;
            }
            if (speedX > 0 && centerX > 200)
            {
                bg1.setSpeedX(-MOVESPEED / 5);
                bg2.setSpeedX(-MOVESPEED / 5);
            }

            // Updates Y Position
            centerY += speedY;

            // Handles Jumping

            speedY += 1;

            if (speedY > 3)
            {
                jumped = true;
            }

            // Prevents going beyond X coordinate of 0
            if (centerX + speedX <= 60)
            {
                centerX = 61;
            }

            rect.Set(centerX - 34, centerY - 63, centerX + 34, centerY);
            rect2.Set(rect.Left, rect.Top + 63, rect.Left + 68, rect.Top + 128);
            rect3.Set(rect.Left - 26, rect.Top + 32, rect.Left, rect.Top + 52);
            rect4.Set(rect.Left + 68, rect.Top + 32, rect.Left + 94, rect.Top + 52);
            yellowRed.Set(centerX - 110, centerY - 110, centerX + 70, centerY + 70);
            footleft.Set(centerX - 50, centerY + 20, centerX, centerY + 35);
            footright.Set(centerX, centerY + 20, centerX + 50, centerY + 35);


        }

        public void moveRight()
        {
            if (ducked == false)
            {
                speedX = MOVESPEED;
            }
        }

        public void moveLeft()
        {
            if (ducked == false)
            {
                speedX = -MOVESPEED;
            }
        }

        public void stopRight()
        {
            setMovingRight(false);
            stop();
        }

        public void stopLeft()
        {
            setMovingLeft(false);
            stop();
        }

        private void stop()
        {
            if (isMovingRight() == false && isMovingLeft() == false)
            {
                speedX = 0;
            }

            if (isMovingRight() == false && isMovingLeft() == true)
            {
                moveLeft();
            }

            if (isMovingRight() == true && isMovingLeft() == false)
            {
                moveRight();
            }

        }

        public void jump()
        {
            if (jumped == false)
            {
                speedY = JUMPSPEED;
                jumped = true;
            }

        }

        public void shoot()
        {
            if (readyToFire)
            {
                Projectile p = new Projectile(centerX + 50, centerY - 25);
                projectiles.Add(p);
            }
        }

        public int getCenterX()
        {
            return centerX;
        }

        public int getCenterY()
        {
            return centerY;
        }

        public bool isJumped()
        {
            return jumped;
        }

        public int getSpeedX()
        {
            return speedX;
        }

        public int getSpeedY()
        {
            return speedY;
        }

        public void setCenterX(int centerX)
        {
            this.centerX = centerX;
        }

        public void setCenterY(int centerY)
        {
            this.centerY = centerY;
        }

        public void setJumped(bool jumped)
        {
            this.jumped = jumped;
        }

        public void setSpeedX(int speedX)
        {
            this.speedX = speedX;
        }

        public void setSpeedY(int speedY)
        {
            this.speedY = speedY;
        }

        public bool isDucked()
        {
            return ducked;
        }

        public void setDucked(bool ducked)
        {
            this.ducked = ducked;
        }

        public bool isMovingRight()
        {
            return movingRight;
        }

        public void setMovingRight(bool movingRight)
        {
            this.movingRight = movingRight;
        }

        public bool isMovingLeft()
        {
            return movingLeft;
        }

        public void setMovingLeft(bool movingLeft)
        {
            this.movingLeft = movingLeft;
        }

        public ArrayList<Projectile> getProjectiles()
        {
            return projectiles;
        }

        public bool isReadyToFire()
        {
            return readyToFire;
        }

        public void setReadyToFire(bool readyToFire)
        {
            this.readyToFire = readyToFire;
        }

    }
}