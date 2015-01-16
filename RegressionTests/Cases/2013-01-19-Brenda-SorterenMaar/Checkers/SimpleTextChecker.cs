using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SorterenMaar.Palette;
using SorterenMaar.Games;

namespace SorterenMaar.Checkers
{

  public class SimpleTextData : ICheckerData
  {
    public string Text { get; set; }
  }
    public delegate void TaskFinished(bool result);

  public class SimpleTextChecker
  {
    Dictionary<string, List<string>> dict = new Dictionary<string,List<string>>();
    public MoveDropAccepted DropHandler { get; set; }
    public int NrAccepts { get; set; }

    public event TaskFinished OnTaskFinished;

    int nrAcceptedDrops;

    public SimpleTextChecker()
    {
      DropHandler = new MoveDropAccepted(DropAcceptedHandler);
      nrAcceptedDrops = 0;
      NrAccepts = 0;
    }


    public void DropAcceptedHandler(string oldContainerId, string newContainerId, ICheckerData data)
    {
      if (!(data is SimpleTextData))
      {
        throw new ArgumentException("SimpleTextChecker expects data of type SimpleTextData");
      }

      var d = data as SimpleTextData;

      if (!dict.ContainsKey(newContainerId))
      {
        dict.Add(newContainerId, new List<string>());
      }
      dict[newContainerId].Add(d.Text);

      if (oldContainerId == "default")
      {
        ++nrAcceptedDrops;
      }
      else
      {
        dict[oldContainerId].Remove(d.Text);
      }

      if (nrAcceptedDrops == NrAccepts)
      {
        // Check the result
        var r = CheckResults();

        // Inform the game all drop are done
        OnTaskFinished(r);
      }
    }

    public bool CheckResults()
    {
      bool res = true;
      // Check that all results in a container are the same
      foreach (var c in dict)
      {
        res &= (c.Value.Distinct().Count() == 1);
      }
      return res;
    }

    public ICheckerData CreateCheckerData(string data)
    {
      return new SimpleTextData { Text = data };
    }
  }
}
