using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rekenspel.Interfaces;
using RekenMaar.Games;


namespace Rekenspel.Games
{
  class AdditionGame : IGame
  {
    int upperLimit; 
    int counterMin; 
    int counterMax;
    bool randomlyReverseOperands;
    int operand1;
    int operand2;
    RandomValueGenerator randomGenerator;

    public AdditionGame(AddGameParameters gameParameters)
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
      operand1 = randomGenerator.GetRandomNumberBetween(counterMin, upperLimit - operand2);
      assignment = operand1.ToString() + " + " + operand2.ToString();

      return assignment;
    }


    public bool CheckAnswer(string answer)
    {
      int answerValue = operand1 + operand2;
      return (answer.CompareToIgnoreCase(answerValue.ToString()) == 0);
    }


  }
}
