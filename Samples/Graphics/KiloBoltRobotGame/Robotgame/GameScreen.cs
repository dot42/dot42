using System;
using Android.Graphics;
using Java.Util;
using KiloBoltRobotGame.Framework;

namespace KiloBoltRobotGame.Robotgame
{
    public class GameScreen : Screen
    {
        internal enum GameState
        {
            Ready,
            Running,
            Paused,
            GameOver
        }

        private GameState state = GameState.Ready;

        // Variable Setup

        private static Background bg1, bg2;
        private static Robot robot;
        public static Heliboy hb, hb2;

        private IImage currentSprite,
                       character,
                       character2,
                       character3,
                       heliboy,
                       heliboy2,
                       heliboy3,
                       heliboy4,
                       heliboy5;

        private Animation anim, hanim;

        private ArrayList<Tile> tilearray = new ArrayList<Tile>();

        private int livesLeft = 1;
        private Paint paint1, paint2;

        public GameScreen(IGame game)
            : base(game)
        {
            // Initialize game objects here

            bg1 = new Background(0, 0);
            bg2 = new Background(2160, 0);
            robot = new Robot();
            hb = new Heliboy(340, 360);
            hb2 = new Heliboy(700, 360);

            character = Assets.character;
            character2 = Assets.character2;
            character3 = Assets.character3;

            heliboy = Assets.heliboy;
            heliboy2 = Assets.heliboy2;
            heliboy3 = Assets.heliboy3;
            heliboy4 = Assets.heliboy4;
            heliboy5 = Assets.heliboy5;

            anim = new Animation();
            anim.addFrame(character, 1250);
            anim.addFrame(character2, 50);
            anim.addFrame(character3, 50);
            anim.addFrame(character2, 50);

            hanim = new Animation();
            hanim.addFrame(heliboy, 100);
            hanim.addFrame(heliboy2, 100);
            hanim.addFrame(heliboy3, 100);
            hanim.addFrame(heliboy4, 100);
            hanim.addFrame(heliboy5, 100);
            hanim.addFrame(heliboy4, 100);
            hanim.addFrame(heliboy3, 100);
            hanim.addFrame(heliboy2, 100);

            currentSprite = anim.getImage();

            loadMap();

            // Defining a paint object
            paint1 = new Paint();
            paint1.SetTextSize(30);
            paint1.SetTextAlign(Paint.Align.CENTER);
            paint1.SetAntiAlias(true);
            paint1.SetColor(Color.WHITE);

            paint2 = new Paint();
            paint2.SetTextSize(100);
            paint2.SetTextAlign(Paint.Align.CENTER);
            paint2.SetAntiAlias(true);
            paint2.SetColor(Color.WHITE);

        }

        private void loadMap()
        {
            ArrayList<string> lines = new ArrayList<string>();
            int width = 0;
            int height = 0;

            var scanner = new Scanner(SampleGame.map);
            while (scanner.HasNextLine())
            {
                string line = scanner.NextLine();

                // no more lines to read
                if (line == null)
                {
                    break;
                }

                if (!line.StartsWith("!"))
                {
                    lines.Add(line);
                    width = Math.Max(width, line.Length);

                }
            }
            height = lines.Size();

            for (int j = 0; j < 12; j++)
            {
                string line = (string) lines.Get(j);
                for (int i = 0; i < width; i++)
                {

                    if (i < line.Length)
                    {
                        char ch = line[i];
                        Tile t = new Tile(i, j, char.GetNumericValue(ch));
                        tilearray.Add(t);
                    }

                }
            }

        }

        public override void update(float deltaTime)
        {
            IList<TouchEvent> touchEvents = game.getInput().getTouchEvents();

            // We have four separate update methods in this example.
            // Depending on the state of the game, we call different update methods.
            // Refer to Unit 3's code. We did a similar thing without separating the
            // update methods.

            if (state == GameState.Ready)
                updateReady(touchEvents);
            if (state == GameState.Running)
                updateRunning(touchEvents, deltaTime);
            if (state == GameState.Paused)
                updatePaused(touchEvents);
            if (state == GameState.GameOver)
                updateGameOver(touchEvents);
        }

        private void updateReady(IList<TouchEvent> touchEvents)
        {

            // This example starts with a "Ready" screen.
            // When the user touches the screen, the game begins.
            // state now becomes GameState.Running.
            // Now the updateRunning() method will be called!

            if (touchEvents.Size() > 0)
                state = GameState.Running;
        }

        private void updateRunning(IList<TouchEvent> touchEvents, float deltaTime)
        {

            // This is identical to the update() method from our Unit 2/3 game.

            // 1. All touch input is handled here:
            int len = touchEvents.Size();
            for (int i = 0; i < len; i++)
            {
                TouchEvent @event = touchEvents.Get(i);
                if (@event.type == TouchEvent.TOUCH_DOWN)
                {

                    if (inBounds(@event, 0, 285, 65, 65))
                    {
                        robot.jump();
                        currentSprite = anim.getImage();
                        robot.setDucked(false);
                    }

                    else if (inBounds(@event, 0, 350, 65, 65))
                    {

                        if (robot.isDucked() == false && robot.isJumped() == false
                            && robot.isReadyToFire())
                        {
                            robot.shoot();
                        }
                    }

                    else if (inBounds(@event, 0, 415, 65, 65) && robot.isJumped() == false)
                    {
                        currentSprite = Assets.characterDown;
                        robot.setDucked(true);
                        robot.setSpeedX(0);

                    }

                    if (@event.x > 400)
                    {
                        // Move right.
                        robot.moveRight();
                        robot.setMovingRight(true);

                    }

                }

                if (@event.
                        type == TouchEvent.TOUCH_UP)
                {

                    if (inBounds(@event, 0, 415, 65, 65))
                    {
                        currentSprite = anim.getImage();
                        robot.setDucked(false);

                    }

                    if (inBounds(@event, 0, 0, 35, 35))
                    {
                        pause();

                    }

                    if (@event.x > 400)
                    {
                        // Move right.
                        robot.stopRight();
                    }
                }

            }

            // 2. Check miscellaneous events like death:

            if (livesLeft == 0)
            {
                state = GameState.GameOver;
            }

            // 3. Call individual update() methods here.
            // This is where all the game updates happen.
            // For example, robot.update();
            robot.update();
            if (robot.isJumped())
            {
                currentSprite = Assets.characterJump;
            }
            else if (robot.isJumped() == false && robot.isDucked() == false)
            {
                currentSprite = anim.getImage();
            }

            ArrayList<Projectile> projectiles = robot.getProjectiles();
            for (int i = 0; i < projectiles.Size(); i++)
            {
                Projectile p = (Projectile) projectiles.Get(i);
                if (p.isVisible() == true)
                {
                    p.update();
                }
                else
                {
                    projectiles.Remove(i);
                }
            }

            updateTiles();
            hb.update();
            hb2.update();
            bg1.update();
            bg2.update();
            animate();

            if (robot.getCenterY() > 500)
            {
                state = GameState.GameOver;
            }
        }

        private bool inBounds(TouchEvent @event, int x, int y, int width, int height)
        {
            if (@event.x > x && @event.x < x + width - 1 && @event.y > y && @event.y < y + height - 1)
                return true;
            else
                return false;
        }

        private void updatePaused(IList<TouchEvent> touchEvents)
        {
            int len = touchEvents.Size();
            for (int i = 0; i < len; i++)
            {
                TouchEvent @event = touchEvents.Get(i);
                if (@event.type == TouchEvent.TOUCH_UP)
                {
                    if (inBounds(@event, 0, 0, 800, 240))
                    {

                        if (!inBounds(@event,0,0,35,35))
                        {
                            resume();
                        }
                    }

                    if (inBounds(@event,0,240,800,240))
                    {
                        nullify();
                        goToMenu();
                    }
                }
            }
        }

        private void updateGameOver(IList<TouchEvent> touchEvents)
        {
            int len = touchEvents.Size();
            for (int i = 0; i < len; i++)
            {
                TouchEvent @event = touchEvents.Get(i);
                if (@event.type == TouchEvent.TOUCH_DOWN)
                {
                    if (inBounds(@event, 0, 0, 800, 480))
                    {
                        nullify();
                        game.setScreen(new MainMenuScreen(game));
                        return;
                    }
                }
            }

        }

        private void updateTiles()
        {

            for (int i = 0; i < tilearray.Size(); i++)
            {
                Tile t = (Tile) tilearray.Get(i);
                t.update();
            }

        }

        public override void paint(float deltaTime)
        {
            var g = game.getGraphics();

            g.drawImage(Assets.background, bg1.getBgX(), bg1.getBgY());
            g.drawImage(Assets.background, bg2.getBgX(), bg2.getBgY());
            paintTiles(g);

            var projectiles = robot.getProjectiles();
            for (int i = 0; i < projectiles.Size(); i++)
            {
                Projectile p = (Projectile) projectiles.Get(i);
                g.drawRect(p.getX(), p.getY(), 10, 5, Color.YELLOW);
            }
            // First draw the game elements.

            g.drawImage(currentSprite, robot.getCenterX() - 61,
                        robot.getCenterY() - 63);
            g.drawImage(hanim.getImage(), hb.getCenterX() - 48,
                        hb.getCenterY() - 48);
            g.drawImage(hanim.getImage(), hb2.getCenterX() - 48,
                        hb2.getCenterY() - 48);

            // Example:
            // g.drawImage(Assets.background, 0, 0);
            // g.drawImage(Assets.character, characterX, characterY);

            // Secondly, draw the UI above the game elements.
            if (state == GameState.Ready)
                drawReadyUI();
            if (state == GameState.Running)
                drawRunningUI();
            if (state == GameState.Paused)
                drawPausedUI();
            if (state == GameState.GameOver)
                drawGameOverUI();

        }

        private void paintTiles(IGraphics g)
        {
            for (int i = 0; i < tilearray.Size(); i++)
            {
                Tile t = (Tile) tilearray.Get(i);
                if (t.type != 0)
                {
                    g.drawImage(t.getTileImage(), t.getTileX(), t.getTileY());
                }
            }
        }

        public void animate()
        {
            anim.update(10);
            hanim.update(50);
        }

        private void nullify()
        {

            // Set all variables to null. You will be recreating them in the
            // constructor.
            paint1 = null;
            bg1 = null;
            bg2 = null;
            robot = null;
            hb = null;
            hb2 = null;
            currentSprite = null;
            character = null;
            character2 = null;
            character3 = null;
            heliboy = null;
            heliboy2 = null;
            heliboy3 = null;
            heliboy4 = null;
            heliboy5 = null;
            anim = null;
            hanim = null;

            // Call garbage collector to clean up memory.
            Java.Lang.System.Gc();

        }

        private void drawReadyUI()
        {
            var g = game.getGraphics();

            g.drawARGB(155, 0, 0, 0);
            g.drawString("Tap to Start.", 400, 240, paint1);
        }

        private void drawRunningUI()
        {
            IGraphics g = game.getGraphics();
            g.drawImage(Assets.button, 0, 285, 0, 0, 65, 65);
            g.drawImage(Assets.button, 0, 350, 0, 65, 65, 65);
            g.drawImage(Assets.button, 0, 415, 0, 130, 65, 65);
            g.drawImage(Assets.button, 0, 0, 0, 195, 35, 35);

        }

        private void drawPausedUI()
        {
            IGraphics g = game.getGraphics();
            // Darken the entire screen so you can display the Paused screen.
            g.drawARGB(155, 0, 0, 0);
            g.drawString("Resume", 400, 165, paint2);
            g.drawString("Menu", 400, 360, paint2);

        }

        private void drawGameOverUI()
        {
            IGraphics g = game.getGraphics();
            g.drawRect(0, 0, 1281, 801, Color.BLACK);
            g.drawString("GAME OVER.", 400, 240, paint2);
            g.drawString("Tap to return.", 400, 290, paint1);

        }

        public override void pause()
        {
            if (state == GameState.Running)
                state = GameState.Paused;

        }

        public override void resume()
        {
            if (state == GameState.Paused)
                state = GameState.Running;
        }

        public override void dispose()
        {

        }

        public override void backButton()
        {
            pause();
        }

        private void goToMenu()
        {
            // TODO Auto-generated method stub
            game.setScreen(new MainMenuScreen(game));

        }

        public static Background getBg1()
        {
            // TODO Auto-generated method stub
            return bg1;
        }

        public static Background getBg2()
        {
            // TODO Auto-generated method stub
            return bg2;
        }

        public static Robot getRobot()
        {
            // TODO Auto-generated method stub
            return robot;
        }

    }
}