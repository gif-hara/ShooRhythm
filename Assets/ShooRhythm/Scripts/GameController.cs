using System.Threading;
using Cysharp.Threading.Tasks;
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
            gameData.Stats.OnChangedAsObservable(cancellationToken)
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

        public UniTask<bool> AddStats(string name, int value)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            gameData.Stats.Add(name, value);
            Debug.Log(gameData.Stats);
            return UniTask.FromResult(true);
        }

        public UniTask<bool> SetStatsAsync(string name, int value)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            gameData.Stats.Set(name, value);
            Debug.Log(gameData.Stats);
            return UniTask.FromResult(true);
        }

        public UniTask<bool> CollectingAsync(Contents.Record collection)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            collection.ApplyRewards(gameData.Stats);
            Debug.Log(gameData.Stats);
            return UniTask.FromResult(true);
        }
    }
}
