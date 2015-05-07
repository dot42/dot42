using Android.Views;

namespace SorterenMaar.Games
{
  public interface ITask
  {
    View CreateView(LayoutInflater inflater, ViewGroup container);
    //void GameFinished();
    //void CreateNextGame();
  }

  public interface IDropEvents
  {
    void DropAccepted();
  }

  public interface ICheckerData
  {
  }

}
