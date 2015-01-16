using RekenMaar.Games;
using Rekenspel.Interfaces;

namespace Rekenspel.Games
{
  class SubtractionGame : IGame
  {
    int upperLimit;
    int counterMin;
    int counterMax;
    bool randomlyReverseOperands;
    int operand1;
    int operand2;
    RandomValueGenerator randomGenerator;

    public SubtractionGame(AddGameParameters gameParameters)
    {
      upperLimit = gameParameters.GetUpperLimit();
      counterMin = gameParameters.GetCounterMin();
      counterMax = gameParameters.GetCounterMax();
      randomlyReverseOperands = gameParameters.GetRandomlyReverseOperands();
      randomGenerator = new RandomValueGenerator();
    }

    public string GetNextAssignment()
    {
      string assignment = "";

      operand2 = randomGenerator.GetRandomNumberBetween(counterMin, counterMax);
      operand1 = randomGenerator.GetRandomNumberBetween(counterMin, upperLimit);
      if (operand2 > operand1)
      {
        int x = operand1;
        operand1 = operand2;
        operand2 = x;
      }
      assignment = operand1.ToString() + " - " + operand2.ToString();

      return assignment;
    }

    public bool CheckAnswer(string answer)
    {
      int answerValue = operand1 - operand2;
      return (answer.CompareToIgnoreCase(answerValue.ToString()) == 0);
    }

  }
}
