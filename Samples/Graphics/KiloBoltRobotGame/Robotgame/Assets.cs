using KiloBoltRobotGame.Framework;

namespace KiloBoltRobotGame.Robotgame
{
    public static class Assets
    {
        public const string Prefix = "KiloBoltRobotGame.Assets.";

        public static IImage menu,
                            splash,
                            background,
                            character,
                            character2,
                            character3,
                            heliboy,
                            heliboy2,
                            heliboy3,
                            heliboy4,
                            heliboy5;

        public static IImage tiledirt,
                            tilegrassTop,
                            tilegrassBot,
                            tilegrassLeft,
                            tilegrassRight,
                            characterJump,
                            characterDown;

        public static IImage button;
        public static ISound click;
        public static IMusic theme;

        public static void load(SampleGame sampleGame)
        {
            // TODO Auto-generated method stub
            theme = sampleGame.getAudio().createMusic(Prefix + "menutheme.mp3");
            theme.setLooping(true);
            theme.setVolume(0.85f);
            theme.play();
        }

    }
}