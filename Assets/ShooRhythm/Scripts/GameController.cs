using System.Threading;
using HK;
using R3;
using SCD;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameController
    {
        public GameController(CancellationToken cancellationToken)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            gameData.Stats.OnChanged.AsObservable()
                .Subscribe(x =>
                {
                    var startString = "Item.";
                    if (x.Name.StartsWith(startString, System.StringComparison.Ordinal))
                    {
                        var id = int.Parse(x.Name.Substring(startString.Length));
                        gameData.SetItem(id, x.Value);
                    }
                })
                .RegisterTo(cancellationToken);
        }
        public void AddStats(string name, int value)
        {
            TinyServiceLocator.Resolve<GameData>().Stats.Add(name, value);
        }

        public void SetStats(string name, int value)
        {
            TinyServiceLocator.Resolve<GameData>().Stats.Set(name, value);
        }
    }
}
