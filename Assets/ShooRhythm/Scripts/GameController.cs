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
                    ObserveProductionMachineSlot(machineId, 0);
                    ObserveProductionMachineSlot(machineId, 1);
                    ObserveProductionMachineSlot(machineId, 2);
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
                var itemIds = new[]
                {
                    gameData.Stats.Get($"Productions.Machine.{machineId}.Slot.0.ItemId"),
                    gameData.Stats.Get($"Productions.Machine.{machineId}.Slot.1.ItemId"),
                    gameData.Stats.Get($"Productions.Machine.{machineId}.Slot.2.ItemId"),
                }
                .Where(x => x != 0);
                if (!itemIds.Any())
                {
                    return UniTask.CompletedTask;
                }

                var conditionNames = itemIds.Select(x => $"Item.{x}").ToArray();

                var colletion = TinyServiceLocator.Resolve<MasterData>().Collections.Records
                    .FirstOrDefault(x =>
                    {
                        if (x.Conditions.Count != conditionNames.Length)
                        {
                            return false;
                        }
                        return !x.Conditions.Select(y => y.Name).Except(conditionNames).Any();
                    });
                if (colletion == null)
                {
                    return UniTask.CompletedTask;
                }
                var collectionSpec = TinyServiceLocator.Resolve<MasterData>().CollectionSpecs.Get(int.Parse(colletion.Name));
                Assert.IsNotNull(collectionSpec, $"CollectionSpec is null. CollectionId:{colletion.Name}");
                return SetStatsAsync($"Productions.Machine.{machineId}.Product", collectionSpec.AcquireItemId);
            }
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
