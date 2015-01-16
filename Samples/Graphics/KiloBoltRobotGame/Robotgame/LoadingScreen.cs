using KiloBoltRobotGame.Framework;

namespace KiloBoltRobotGame.Robotgame
{
    public class LoadingScreen : Screen
{
    public LoadingScreen(IGame game) : base(game)
    {
    }

    public override void update(float deltaTime)
    {
        IGraphics g = game.getGraphics();
        Assets.menu = g.newImage("menu.png", ImageFormat.RGB565);
        Assets.background = g.newImage("background.png", ImageFormat.RGB565);
        Assets.character = g.newImage("character.png", ImageFormat.ARGB4444);
        Assets.character2 = g.newImage("character2.png", ImageFormat.ARGB4444);
        Assets.character3 = g.newImage("character3.png", ImageFormat.ARGB4444);
        Assets.characterJump = g.newImage("jumped.png", ImageFormat.ARGB4444);
        Assets.characterDown = g.newImage("down.png", ImageFormat.ARGB4444);


        Assets.heliboy = g.newImage("heliboy.png", ImageFormat.ARGB4444);
        Assets.heliboy2 = g.newImage("heliboy2.png", ImageFormat.ARGB4444);
        Assets.heliboy3 = g.newImage("heliboy3.png", ImageFormat.ARGB4444);
        Assets.heliboy4 = g.newImage("heliboy4.png", ImageFormat.ARGB4444);
        Assets.heliboy5 = g.newImage("heliboy5.png", ImageFormat.ARGB4444);



        Assets.tiledirt = g.newImage("tiledirt.png", ImageFormat.RGB565);
        Assets.tilegrassTop = g.newImage("tilegrasstop.png", ImageFormat.RGB565);
        Assets.tilegrassBot = g.newImage("tilegrassbot.png", ImageFormat.RGB565);
        Assets.tilegrassLeft = g.newImage("tilegrassleft.png", ImageFormat.RGB565);
        Assets.tilegrassRight = g.newImage("tilegrassright.png", ImageFormat.RGB565);

        Assets.button = g.newImage("button.jpg", ImageFormat.RGB565);

        //This is how you would load a sound if you had one.
        //Assets.click = game.getAudio().createSound("explode.ogg");


        game.setScreen(new MainMenuScreen(game));

    }

    public override void paint(float deltaTime)
    {
        IGraphics g = game.getGraphics();
        g.drawImage(Assets.splash, 0, 0);
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