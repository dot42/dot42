using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekenspel.Interfaces;
using RekenMaar.Activities;
using RekenMaar.Games;

namespace Rekenspel.Games
{
  class MultiplicationTableGame : IGame
  {
    int tableOf = 1;
    bool eenTafel = true;
    int multiplicand = 0;
    int multiplier = 0;
    RandomValueGenerator randomGenerator;

    public MultiplicationTableGame(MultiplyGameParameters gameParameters)
    {
      tableOf = gameParameters.GetTableOf();
      eenTafel = gameParameters.GetIsRandom();
      randomGenerator = new RandomValueGenerator();
    }

    public string GetNextAssignment()
    {
      string assignment;
      if (eenTafel)
      {
        multiplier = tableOf;
      }
      else
      {
        multiplier = randomGenerator.GetRandomNumberBetween(1, tableOf);
      }

      int randomValue = multiplicand;
      while (multiplicand == randomValue)
      {
        randomValue = randomGenerator.GetRandomNumberBetween(1, 10);
      }

      multiplicand = randomValue;
      assignment = multiplicand.ToString() + " x " + multiplier.ToString();

      return assignment;
    }

    public bool CheckAnswer(string answer)
    {
      int answerValue = multiplicand * multiplier;

      return (answer.CompareToIgnoreCase(answerValue.ToString()) == 0);
    }

  }
}
