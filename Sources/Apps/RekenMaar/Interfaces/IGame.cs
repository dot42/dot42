using Android.Os;

namespace Rekenspel.Interfaces
{
  interface IGame
  {
    string GetNextAssignment();
    bool CheckAnswer(string answer);
  }

  interface IGameParameters
  {
    string GetGameType();
    //void StoreParameters();
    //void RetrieveParameters();
  }
}
