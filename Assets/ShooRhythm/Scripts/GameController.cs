using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using SCD;
using UnityEngine;
using UnityEngine.Assertions;

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
            CancellationTokenSource productionMachineScope = default;
            gameData.Stats.OnChangedAsObservable(cancellationToken)
                .Subscribe(x =>
                {
                    var startString = "Item.";
                    if (x.Name.StartsWith(startString, System.StringComparison.Ordinal))
                    {
                        var id = int.Parse(x.Name.Substring(startString.Length));
                        gameData.SetItem(id, x.Value);
                    }
                    if (x.Name == "Productions.MachineNumber")
                    {
                        ObserveProductionMachine();
                    }
                })
                .RegisterTo(cancellationToken);

            ObserveProductionMachine();

            void ObserveProductionMachine()
            {
                productionMachineScope?.Cancel();
                productionMachineScope?.Dispose();
                productionMachineScope = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                for (var i = 0; i < gameData.Stats.Get("Productions.MachineNumber"); i++)
                {
                    var machineId = i;
                    for(var j=0; j<Define.MachineSlotCount; j++)
                    {
                        ObserveProductionMachineSlot(machineId, j);
                    }
                }
            }
            void ObserveProductionMachineSlot(int machineId, int slotId)
            {
                gameData.Stats.OnChangedAsObservable(productionMachineScope.Token)
                    .Subscribe(x =>
                    {
                        if (x.Name == $"Productions.Machine.{machineId}.Slot.{slotId}.ItemId")
                        {
                            TrySetProductionMachineProductAsync(machineId).Forget();
                        }
                    })
                    .RegisterTo(productionMachineScope.Token);
            }
            UniTask TrySetProductionMachineProductAsync(int machineId)
            {
                var conditionNames = Enumerable.Range(0, Define.MachineSlotCount)
                    .Select(x => gameData.Stats.Get($"Productions.Machine.{machineId}.Slot.{x}.ItemId"))
                    .Where(x => x != 0)
                    .Select(x => $"Item.{x}")
                    .ToArray();
                if (!conditionNames.Any())
                {
                    return UniTask.CompletedTask;
                }

                var collection = TinyServiceLocator.Resolve<MasterData>().Collections.Records
                    .FirstOrDefault(x =>
                    {
                        if (x.Conditions.Count != conditionNames.Length)
                        {
                            return false;
                        }
                        return !x.Conditions.Select(y => y.Name).Except(conditionNames).Any();
                    });
                var statsName = $"Productions.Machine.{machineId}.Product";
                if (collection == null)
                {
                    return SetStatsAsync(statsName, 0);
                }
                var collectionSpec = TinyServiceLocator.Resolve<MasterData>().CollectionSpecs.Get(int.Parse(collection.Name));
                Assert.IsNotNull(collectionSpec, $"CollectionSpec is null. CollectionId:{collection.Name}");
                return SetStatsAsync(statsName, collectionSpec.AcquireItemId);
            }
        }

        public UniTask<bool> AddStatsAsync(string name, int value)
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
