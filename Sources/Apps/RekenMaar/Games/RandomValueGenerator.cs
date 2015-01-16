using System;
using Java.Util;

namespace RekenMaar.Games
{
  public class RandomValueGenerator
  {
    Random randomGenerator = new Random();
    public int GetRandomNumberBetween(int from, int to)
    {
      return (int)Math.Floor(randomGenerator.NextDouble() * (to - from + 1) + from);
    }
  }
}
