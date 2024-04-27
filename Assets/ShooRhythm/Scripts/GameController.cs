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
            AddUserData(userId);
            Observable.EveryUpdate(cancellationToken)
                .Subscribe(_ =>
                {
                    for (var i = 0; i < gameData.CurrentUserData.coolTimeData.Count; i++)
                    {
                        var coolTimeData = gameData.CurrentUserData.coolTimeData[i];
#if DEBUG
                        if (TinyServiceLocator.Resolve<GameDebugData>().IgnoreCoolDown)
                        {
                            coolTimeData.CoolTime.Value = 0;
                        }
#endif

                        coolTimeData.CoolTime.Value -= Time.deltaTime;
                        if (coolTimeData.CoolTime.Value < 0)
                        {
                            coolTimeData.CoolTime.Value = 0;
                        }
                    }
                })
                .RegisterTo(cancellationToken);
        }

        public UniTask<Define.ProcessResultType> SetUserEquipmentItemIdAsync(int userId, int x)
        {
            TinyServiceLocator.Resolve<GameData>().UserData[userId].equipmentItemId.Value = x;
            return UniTask.FromResult(Define.ProcessResultType.Success);
        }

        public UniTask<Define.ProcessResultType> ProcessFarmSetPlantItemIdAsync(int plantId, int seedItemId)
        {
            var seedSpec = TinyServiceLocator.Resolve<MasterData>().SeedSpecs.Get(seedItemId);
            var gameData = TinyServiceLocator.Resolve<GameData>();
            if (seedSpec.HasItems())
            {
                AddItemAsync(seedSpec.SeedItemId, -1).Forget();
                gameData.FarmDatas[plantId].SeedItemId.Value = seedSpec.Id;
                gameData.FarmDatas[plantId].PlantTicks.Value = DateTime.UtcNow.Ticks;
                return UniTask.FromResult(Define.ProcessResultType.Success);
            }
            else
            {
                return UniTask.FromResult(Define.ProcessResultType.NotEnoughItem);
            }
        }

        public UniTask<Define.ProcessResultType> ProcessFarmAcquirePlantItemAsync(int plantId)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var seedSpec = gameData.FarmDatas[plantId].SeedSpec;
            if (gameData.FarmDatas[plantId].IsCompleted)
            {
                AddItemAsync(seedSpec.AcquireItemId, 1).Forget();
                gameData.FarmDatas[plantId].SeedItemId.Value = 0;
                gameData.FarmDatas[plantId].PlantTicks.Value = 0;
                return UniTask.FromResult(Define.ProcessResultType.Success);
            }
            else
            {
                return UniTask.FromResult(Define.ProcessResultType.FarmTimerInProgress);
            }
        }

        public async UniTask<Define.ProcessResultType> ProcessBattleGetEnemyInstanceDataAsync(Define.DungeonType dungeonType)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            if (gameData.DungeonEnemyInstanceDatas.TryGetValue(dungeonType, out var enemyInstanceData))
            {
                return Define.ProcessResultType.Success;
            }

            var result = await CreateEnemyInstanceDataAsync(dungeonType);
            gameData.DungeonEnemyInstanceDatas[dungeonType] = result;
            return Define.ProcessResultType.Success;
        }

        public async UniTask<Define.AttackResultType> ProcessBattleAttackEnemyAsync(Define.DungeonType dungeonType, int damage)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var enemyInstanceData = gameData.DungeonEnemyInstanceDatas[dungeonType];
            enemyInstanceData.HitPoint -= damage;
            if (enemyInstanceData.HitPoint <= 0)
            {
                var enemySpec = TinyServiceLocator.Resolve<MasterData>().EnemySpecs.Get(dungeonType)
                    .FirstOrDefault(x => x.Id == enemyInstanceData.EnemyId);
                var result = await UniTask.WhenAll(
                    AddItemAsync(enemySpec.RewardItemId, enemySpec.RewardItemAmount),
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

        public UniTask<Define.ProcessResultType> AddAvailableContentAsync(string availableContent)
        {
            TinyServiceLocator.Resolve<GameData>().AvailableContents.Add(availableContent);
            TinyServiceLocator.Resolve<GameMessage>().AddedContentAvailability.OnNext(availableContent);
            return UniTask.FromResult(Define.ProcessResultType.Success);
        }

        public UniTask<Define.ProcessResultType> DebugAddItemAsync(int itemId, int amount)
        {
            return AddItemAsync(itemId, amount);
        }

        public UniTask<Define.ProcessResultType> ProcessCollectionAsync(int collectionSpecId)
        {
            var collectionSpec = TinyServiceLocator.Resolve<MasterData>().CollectionSpecs.Get(collectionSpecId);
            return AddItemAsync(collectionSpec.AcquireItemId, collectionSpec.AcquireItemAmount);
        }

        public UniTask<Define.ProcessResultType> ProcessProductionSetSlotAsync(int machineId, int slotId, int itemId)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var productMachineData = gameData.ProductMachineData[machineId];
            productMachineData.slotItemIds[slotId].Value = itemId;
            var slotItemIds = productMachineData.slotItemIds
                .Where(x => x.Value != 0)
                .Select(x => x.Value)
                .ToArray();
            if (slotItemIds.Any())
            {
                var productionSpec = TinyServiceLocator.Resolve<MasterData>().ProductionSpecs.List
                    .FirstOrDefault(x =>
                    {
                        var conditions = x.GetProductionNeedItems();
                        if (conditions.Count != slotItemIds.Length)
                        {
                            return false;
                        }
                        return !conditions.Select(y => y.NeedItemId).Except(slotItemIds).Any();
                    });
                productMachineData.productItemId.Value = productionSpec?.AcquireItemId ?? 0;
            }

            return UniTask.FromResult(Define.ProcessResultType.Success);
        }

        public async UniTask<Define.ProcessResultType> ProcessProductionAcquireProductAsync(int itemId)
        {
            var productionSpec = TinyServiceLocator.Resolve<MasterData>().ProductionSpecs.Get(itemId);
            var needItems = productionSpec.GetProductionNeedItems();
            if (needItems.IsAllPossession(TinyServiceLocator.Resolve<GameData>()))
            {
                var tasks = needItems
                    .Select(x => AddItemAsync(x.NeedItemId, -x.NeedItemAmount))
                    .Concat(new[]
                    {
                        AddItemAsync(productionSpec.AcquireItemId, productionSpec.AcquireItemAmount)
                    });
                var result = await UniTask.WhenAll(tasks);
                return result.All(x => x == Define.ProcessResultType.Success)
                    ? Define.ProcessResultType.Success
                    : Define.ProcessResultType.Unknown;
            }
            else
            {
                return Define.ProcessResultType.NotEnoughItem;
            }
        }

        public UniTask<Define.ProcessResultType> ProcessRiverFishingAsync(int riverFishingId)
        {
            var fishingSpec = TinyServiceLocator.Resolve<MasterData>().RiverFishingSpecs.Get(riverFishingId);
            return AddItemAsync(fishingSpec.AcquireItemId, fishingSpec.AcquireItemAmount);
        }

        public UniTask<Define.ProcessResultType> ProcessSeaFishingAsync(int seaFishingId)
        {
            var fishingSpec = TinyServiceLocator.Resolve<MasterData>().SeaFishingSpecs.Get(seaFishingId);
            return AddItemAsync(fishingSpec.AcquireItemId, fishingSpec.AcquireItemAmount);
        }

        public async UniTask<Define.ProcessResultType> ProcessMeadowAsync(int meadowId)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var meadowSpec = TinyServiceLocator.Resolve<MasterData>().MeadowSpecs.Get(meadowId);
            if (meadowSpec.HasItems())
            {
                var results = await UniTask.WhenAll(
                    AddItemAsync(meadowSpec.NeedItemId, -meadowSpec.NeedItemAmount),
                    AddItemAsync(meadowSpec.AcquireItemId, meadowSpec.AcquireItemAmount)
                );

                return (results.Item1 == Define.ProcessResultType.Success && results.Item2 == Define.ProcessResultType.Success)
                    ? Define.ProcessResultType.Success
                    : Define.ProcessResultType.Unknown;
            }
            else
            {
                return Define.ProcessResultType.NotEnoughItem;
            }
        }

        public async UniTask<Define.ProcessResultType> ProcessQuestAsync(int questSpecId)
        {
            var questSpec = TinyServiceLocator.Resolve<MasterData>().QuestSpecs.Get(questSpecId);
            var conditions = questSpec.GetQuestConditions();
            if (conditions.IsAllPossession(TinyServiceLocator.Resolve<GameData>()))
            {
                var rewards = questSpec.GetQuestRewards();
                var tasks = rewards
                    .Select(x => AddAvailableContentAsync(x.Name));
                var result = await UniTask.WhenAll(tasks);
                return result.All(x => x == Define.ProcessResultType.Success) ? Define.ProcessResultType.Success : Define.ProcessResultType.Unknown;
            }
            else
            {
                return Define.ProcessResultType.NotEnoughItem;
            }
        }

        private UniTask<Define.ProcessResultType> AddItemAsync(int itemId, int amount)
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

            return UniTask.FromResult(Define.ProcessResultType.Success);
        }
    }
}
