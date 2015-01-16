using Android.Os;
using Rekenspel.Interfaces;
using RekenMaar.Activities;

namespace Rekenspel.Games
{
  class GameParameters : IGameParameters
  {
    string gameType;


    protected GameParameters(string _gameType)
    {
      gameType = _gameType;
    }

    public string GetGameType() { return gameType; }

    static public IGameParameters CreateDefaultGameParameters(string gameType)
    {
      var gameParameters = new GameParameters(gameType);

      return (IGameParameters)gameParameters;
    }

  }

  class MultiplyGameParameters : GameParameters
  {
    MultiplyGameParameters(string gameType)
      : base(gameType) {}

    int tableOf;
    bool isRandom;

    public int GetTableOf() { return tableOf;  }
    public bool GetIsRandom() { return isRandom; }

    static public IGameParameters GetParametersFromPreferences(string gameType, Preferences preferences)
    {
      var gameParameters = new MultiplyGameParameters(gameType);
      gameParameters.tableOf = preferences.TableOf;
      gameParameters.isRandom = preferences.RandomQuestions;

      return (IGameParameters)gameParameters;
    }

    public void StoreParameters()
    {
    }

    public void RetrieveParameters()
    {
      tableOf = 10;
      isRandom = true;
    }
  }

  class AddGameParameters : GameParameters
  {
    AddGameParameters(string gameType)
      : base(gameType) {}

    int upperLimit;
    int counterMin;
    int counterMax;
    bool randomlyReverseOperands;

    public int GetUpperLimit() { return upperLimit; }
    public int GetCounterMin() { return counterMin; }
    public int GetCounterMax() { return counterMax; }
    public bool GetRandomlyReverseOperands() { return randomlyReverseOperands; }

    static public IGameParameters GetParametersFromPreferences(string gameType, Preferences preferences)
    {
      var gameParameters = new AddGameParameters(gameType);
      gameParameters.upperLimit = preferences.UpperLimit;
      gameParameters.counterMin = preferences.CounterMin;
      gameParameters.counterMax = preferences.CounterMax;
      gameParameters.randomlyReverseOperands = false;

      return (IGameParameters)gameParameters;
    }

  }

  class GameParameterFactory
  {
    private Preferences preferences;

    public Preferences Preferences
    {
      get { return preferences; }
      set { preferences = value; }
    }
    
    public IGameParameters GetGameParameters(string gameType)
    {
      IGameParameters gameParameters;
      switch (gameType)
      {
        case "Add":
        case "Subtract":
          gameParameters = AddGameParameters.GetParametersFromPreferences(gameType, preferences);
          break;
        case "Multiply":
        case "Divide":
          gameParameters = MultiplyGameParameters.GetParametersFromPreferences(gameType, preferences);
          break;
        default:
          gameParameters = GameParameters.CreateDefaultGameParameters(gameType);
          break;
      }
      return gameParameters;
    }

  }
}
