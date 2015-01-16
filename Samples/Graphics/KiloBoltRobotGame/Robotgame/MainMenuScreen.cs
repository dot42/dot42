using Java.Util;
using KiloBoltRobotGame.Framework;

namespace KiloBoltRobotGame.Robotgame
{
    public class MainMenuScreen : Screen
    {
        public MainMenuScreen(IGame game)
            : base(game)
        {
        }

        public override void update(float deltaTime)
        {
            IGraphics g = game.getGraphics();
            IList<TouchEvent> touchEvents = game.getInput().getTouchEvents();

            int len = touchEvents.Size();
            for (int i = 0; i < len; i++)
            {
                TouchEvent @event = touchEvents.Get(i);
                if (@event.type == TouchEvent.TOUCH_UP)
                {

                    if (inBounds(@event, 50, 350, 250, 450))
                    {
                        game.setScreen(new GameScreen(game));
                    }
                }
            }
        }

        private bool inBounds(TouchEvent @event, int x, int y, int width, int height)
        {
            if (@event.
                    x > x && @event.
                                 x < x + width - 1 && @event.
                                                          y > y
                && @event.
                       y < y + height - 1)
                return true;
            else
                return false;
        }

        public override void paint(float deltaTime)
        {
            IGraphics g = game.getGraphics();
            g.drawImage(Assets.menu, 0, 0);
        }

        public override void pause()
        {
        }

        public override void resume()
        {
        }

        public override void dispose()
        {
        }

        public override void backButton()
        {
            Android.Os.Process.KillProcess(Android.Os.Process.MyPid());
        }
    }
}