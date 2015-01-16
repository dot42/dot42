using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekenspel.Interfaces;
using RekenMaar.Games;

namespace Rekenspel.Games
{
  class DivisionionTableGame : IGame
  {
    int tableOf = 1;
    bool eenTafel = true;
    int quotient = 0;
    int denominator = 0;
    RandomValueGenerator randomGenerator;

    public DivisionionTableGame(MultiplyGameParameters gameParameters)
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
        denominator = tableOf;
      }
      else
      {
        denominator = randomGenerator.GetRandomNumberBetween(1, tableOf );
      }

      int randomValue = quotient;
      while ((quotient * denominator) == (randomValue * denominator))
      {
        randomValue = randomGenerator.GetRandomNumberBetween(1, 10);
      }

      quotient = randomValue;
      assignment = (quotient * denominator).ToString() + " : " + denominator.ToString();

      return assignment;
    }


    public bool CheckAnswer(string answer)
    {
      return (answer.CompareToIgnoreCase(quotient.ToString()) == 0);
    }

  }
}
