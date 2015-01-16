using KiloBoltRobotGame.Framework;

namespace KiloBoltRobotGame.Robotgame
{
    public class SplashLoadingScreen : Screen
    {
        public SplashLoadingScreen(IGame game)
            : base(game)
        {
        }

        public override void update(float deltaTime)
        {
            IGraphics g = game.getGraphics();
            Assets.splash = g.newImage("splash.jpg", ImageFormat.RGB565);


            game.setScreen(new LoadingScreen(game));

        }

        public override void paint(float deltaTime)
        {

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

        }
    }
}