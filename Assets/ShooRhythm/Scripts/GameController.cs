using System;
using System.Linq;
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
        public GameController(int userId, CancellationToken cancellationToken)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            gameData.Stats.OnChangedAsObservable(cancellationToken)
                .Subscribe(x =>
                {
                    var startString = "Item.";
                    if (x.Name.StartsWith(startString, StringComparison.Ordinal))
                    {
                        var id = int.Parse(x.Name.Substring(startString.Length));
                        gameData.SetItem(id, x.Value);
                    }
                })
                .RegisterTo(cancellationToken);
            AddUserData(userId);
            Observable.EveryUpdate(cancellationToken)
                .Subscribe(_ =>
                {
                    for (var i = 0; i < gameData.CurrentUserData.coolTimeData.Count; i++)
                    {
                        var coolTimeData = gameData.CurrentUserData.coolTimeData[i];
                        coolTimeData.CoolTime.Value -= Time.deltaTime;
                        if (coolTimeData.CoolTime.Value < 0)
                        {
                            coolTimeData.CoolTime.Value = 0;
                        }
                    }
                })
                .RegisterTo(cancellationToken);
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
            TinyServiceLocator.Resolve<GameData>().UserData[userId].equipmentItemId.Value = x;
            return UniTask.FromResult(true);
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

        private static UniTask<EnemyInstanceData> CreateEnemyInstanceDataAsync(Define.DungeonType dungeonType)
        {
            var result = new EnemyInstanceData();
            var enemySpecs = TinyServiceLocator.Resolve<MasterData>().EnemySpecs.Get(dungeonType);
            var enemySpec = enemySpecs[UnityEngine.Random.Range(0, enemySpecs.Count)];
            result.EnemyId = enemySpec.Id;
            result.HitPoint = enemySpec.HitPoint;
            return UniTask.FromResult(result);
        }

        public void AddUserData(int userId)
        {
            var initialCoolTimeNumber = TinyServiceLocator.Resolve<GameDesignData>().InitialCoolTimeNumber;
            TinyServiceLocator.Resolve<GameData>().UserData.Add(userId, new UserData(initialCoolTimeNumber));
        }

        public void AddProductMachine()
        {
            TinyServiceLocator.Resolve<GameData>().ProductMachineData.Add(new ProductMachineData());
            TinyServiceLocator.Resolve<GameMessage>().AddedProductMachineData.OnNext(Unit.Default);
        }

        public void AddFarmData()
        {
            TinyServiceLocator.Resolve<GameData>().FarmDatas.Add(new FarmData());
            TinyServiceLocator.Resolve<GameMessage>().AddedFarmData.OnNext(Unit.Default);
        }

        public UniTask<bool> AddContentAvailabilityAsync(string contentAvailability)
        {
            TinyServiceLocator.Resolve<GameData>().ContentAvailabilities.Add(contentAvailability);
            TinyServiceLocator.Resolve<GameMessage>().AddedContentAvailability.OnNext(contentAvailability);
            return UniTask.FromResult(true);
        }

        public UniTask<bool> DebugAddItemAsync(int itemId, int amount)
        {
            return AddItemAsync(itemId, amount);
        }

        public UniTask<bool> ProcessCollectionAsync(int collectionSpecId)
        {
            var collectionSpec = TinyServiceLocator.Resolve<MasterData>().CollectionSpecs.Get(collectionSpecId);
            return AddItemAsync(collectionSpec.AcquireItemId, collectionSpec.AcquireItemAmount);
        }

        public UniTask<bool> ProcessProductionSetSlotAsync(int machineId, int slotId, int itemId)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var productMachineData = gameData.ProductMachineData[machineId];
            productMachineData.slotItemIds[slotId].Value = itemId;
            var conditionNames = productMachineData.slotItemIds
                .Where(x => x.Value != 0)
                .Select(x => x.Value)
                .ToArray();
            if (conditionNames.Any())
            {
                var productionSpec = TinyServiceLocator.Resolve<MasterData>().ProductionSpecs.List
                    .FirstOrDefault(x =>
                    {
                        var conditions = x.GetProductionCondition();
                        if (conditions.Count != conditionNames.Length)
                        {
                            return false;
                        }
                        return !conditions.Select(y => y.NeedItemId).Except(conditionNames).Any();
                    });
                productMachineData.productItemId.Value = productionSpec?.AcquireItemId ?? 0;
            }

            return UniTask.FromResult(true);
        }

        public async UniTask<bool> ProcessProductionAcquireProductAsync(int itemId)
        {
            var productionSpec = TinyServiceLocator.Resolve<MasterData>().ProductionSpecs.Get(itemId);
            var needItems = productionSpec.GetProductionCondition();
            if (needItems.IsAllPossession(TinyServiceLocator.Resolve<GameData>()))
            {
                var tasks = needItems
                    .Select(x => AddItemAsync(x.NeedItemId, -x.NeedItemAmount))
                    .Concat(new[]
                    {
                        AddItemAsync(productionSpec.AcquireItemId, productionSpec.AcquireItemAmount)
                    });
                await UniTask.WhenAll(tasks);
            }

            return true;
        }

        public UniTask<bool> ProcessRiverFishingAsync(int riverFishingId)
        {
            var fishingSpec = TinyServiceLocator.Resolve<MasterData>().RiverFishingSpecs.Get(riverFishingId);
            return AddItemAsync(fishingSpec.AcquireItemId, fishingSpec.AcquireItemAmount);
        }

        public UniTask<bool> ProcessSeaFishingAsync(int seaFishingId)
        {
            var fishingSpec = TinyServiceLocator.Resolve<MasterData>().SeaFishingSpecs.Get(seaFishingId);
            return AddItemAsync(fishingSpec.AcquireItemId, fishingSpec.AcquireItemAmount);
        }

        private UniTask<bool> AddItemAsync(int itemId, int amount)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var gameMessage = TinyServiceLocator.Resolve<GameMessage>();
            if (!gameData.Items.TryGetValue(itemId, out var reactiveProperty))
            {
                gameData.Items.Add(itemId, new ReactiveProperty<int>(amount));
                gameMessage.AddedItem.OnNext(itemId);
            }
            else
            {
                reactiveProperty.Value += amount;
            }

            return UniTask.FromResult(true);
        }
    }
}
