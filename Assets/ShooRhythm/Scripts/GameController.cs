using System;
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
                    if (x.Name.StartsWith(startString, StringComparison.Ordinal))
                    {
                        var id = int.Parse(x.Name.Substring(startString.Length));
                        gameData.SetItem(id, x.Value);
                    }
                    if (x.Name == "Productions.MachineNumber")
                    {
                        ObserveProductionMachine();
                    }
                    if (x.Name == "Farm.PlantNumber")
                    {
                        gameData.FetchFarmData();
                    }
                })
                .RegisterTo(cancellationToken);

            ObserveProductionMachine();
            gameData.FetchFarmData();

            void ObserveProductionMachine()
            {
                productionMachineScope?.Cancel();
                productionMachineScope?.Dispose();
                productionMachineScope = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                for (var i = 0; i < gameData.Stats.Get("Productions.MachineNumber"); i++)
                {
                    var machineId = i;
                    for (var j = 0; j < Define.MachineSlotCount; j++)
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

                var productionSpec = TinyServiceLocator.Resolve<MasterData>().ProductionSpecs.List
                    .FirstOrDefault(x =>
                    {
                        var conditions = x.GetProductionCondition();
                        if (conditions.Count != conditionNames.Length)
                        {
                            return false;
                        }
                        return !conditions.Select(y => y.Name).Except(conditionNames).Any();
                    });
                var statsName = $"Productions.Machine.{machineId}.Product";
                if (productionSpec == null)
                {
                    return SetStatsAsync(statsName, 0);
                }
                return SetStatsAsync(statsName, productionSpec.AcquireItemId);
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

        public UniTask<bool> ApplyRewardAsync(Contents.Record contentsRecord)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            contentsRecord.ApplyRewards(gameData.Stats);
            Debug.Log(gameData.Stats);
            return UniTask.FromResult(true);
        }

        public UniTask<bool> SetUserEquipmentItemIdAsync(int userId, int x)
        {
            return SetStatsAsync($"UserData.{userId}.Equipment.ItemId", x);
        }

        public UniTask<bool> SetFarmPlantItemIdAsync(int plantId, int seedItemId)
        {
            var seedSpec = TinyServiceLocator.Resolve<MasterData>().SeedSpecs.Get(seedItemId);
            var gameData = TinyServiceLocator.Resolve<GameData>();
            gameData.Stats.Add($"Item.{seedItemId}", -1);
            gameData.FarmDatas[plantId].SeedItemId.Value = seedSpec.Id;
            gameData.FarmDatas[plantId].PlantTicks.Value = DateTime.UtcNow.Ticks;
            return UniTask.FromResult(true);
        }

        public UniTask<bool> AcquireFarmPlantAsync(int plantId)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var seedSpec = gameData.FarmDatas[plantId].SeedSpec;
            gameData.Stats.Add($"Item.{seedSpec.AcquireItemId}", 1);
            gameData.FarmDatas[plantId].SeedItemId.Value = 0;
            gameData.FarmDatas[plantId].PlantTicks.Value = 0;
            return UniTask.FromResult(true);
        }

        public async UniTask<EnemyInstanceData> GetEnemyInstanceDataAsync(Define.DungeonType dungeonType)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            if (gameData.DungeonEnemyInstanceDatas.TryGetValue(dungeonType, out var enemyInstanceData))
            {
                return enemyInstanceData;
            }

            var result = await CreateEnemyInstanceDataAsync(dungeonType);
            gameData.DungeonEnemyInstanceDatas[dungeonType] = result;
            return result;
        }

        public async UniTask<Define.AttackResultType> AttackEnemyAsync(Define.DungeonType dungeonType, int damage)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var enemyInstanceData = gameData.DungeonEnemyInstanceDatas[dungeonType];
            enemyInstanceData.HitPoint -= damage;
            if (enemyInstanceData.HitPoint <= 0)
            {
                var enemySpec = TinyServiceLocator.Resolve<MasterData>().EnemySpecs.Get(dungeonType)
                    .FirstOrDefault(x => x.Id == enemyInstanceData.EnemyId);
                var result = await UniTask.WhenAll(
                    ApplyRewardAsync(enemySpec.ToContentsRecord()),
                    CreateEnemyInstanceDataAsync(dungeonType)
                );

                gameData.DungeonEnemyInstanceDatas[dungeonType] = result.Item2;
                return Define.AttackResultType.Defeat;
            }
            return Define.AttackResultType.Hit;
        }

        private UniTask<EnemyInstanceData> CreateEnemyInstanceDataAsync(Define.DungeonType dungeonType)
        {
            var result = new EnemyInstanceData();
            var enemySpecs = TinyServiceLocator.Resolve<MasterData>().EnemySpecs.Get(dungeonType);
            var enemySpec = enemySpecs[UnityEngine.Random.Range(0, enemySpecs.Count)];
            result.EnemyId = enemySpec.Id;
            result.HitPoint = enemySpec.HitPoint;
            return UniTask.FromResult(result);
        }
    }
}
