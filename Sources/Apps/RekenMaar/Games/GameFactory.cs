using Android.Os;
using Rekenspel.Interfaces;

namespace Rekenspel.Games
{
  class GameFactory
  {
    static public IGame GetGame(string gameType, GameParameterFactory gameParameterFactory)
    {
      IGameParameters gameParameters = gameParameterFactory.GetGameParameters(gameType);
      IGame game = null;
      switch (gameType)
      {
        case "Add":
          if (gameParameters is AddGameParameters)
          {
            game = new AdditionGame((AddGameParameters)gameParameters);
          }
          break;
        case "Subtract":
          if (gameParameters is AddGameParameters)
          {
            game = new SubtractionGame((AddGameParameters)gameParameters);
          }
          break;
        case "Multiply":
          if (gameParameters is MultiplyGameParameters)
          {
            game = new MultiplicationTableGame((MultiplyGameParameters)gameParameters);
          }
          break;
        case "Divide":
          if (gameParameters is MultiplyGameParameters)
          {
            game = new DivisionionTableGame((MultiplyGameParameters)gameParameters);
          }
          break;
        default:
          // throw exception
          game = null;
          break;
      }
      return game;
    }
  }
}
