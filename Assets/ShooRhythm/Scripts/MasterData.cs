using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HK;
using SCD;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "MasterData", menuName = "ShooRhythm/MasterData")]
    public sealed class MasterData : ScriptableObject, IBootable
    {
        [SerializeField]
        private Item.DictionaryList items;
        public Item.DictionaryList Items => items;

        [SerializeField]
        private CollectionSpec.DictionaryList collectionSpecs;
        public CollectionSpec.DictionaryList CollectionSpecs => collectionSpecs;

        [SerializeField]
        private Contents collections;
        public Contents Collections => collections;

        [SerializeField]
        private Contents tabContents;

        [SerializeField]
        private GameStartStatsData.DictionaryList gameStartStats;
        public GameStartStatsData.DictionaryList GameStartStats => gameStartStats;

#if UNITY_EDITOR
        [ContextMenu("Update")]
        private async void UpdateMasterData()
        {
            Debug.Log("Begin MasterData Update");
            var database = await UniTask.WhenAll(
                GoogleSpreadSheetDownloader.DownloadAsync("Item"),
                GoogleSpreadSheetDownloader.DownloadAsync("CollectionSpec"),
                GoogleSpreadSheetDownloader.DownloadAsync("CollectionCondition"),
                GoogleSpreadSheetDownloader.DownloadAsync("CollectionReward"),
                GoogleSpreadSheetDownloader.DownloadAsync("GameStartStats")
            );
            items.Set(JsonHelper.FromJson<Item>(database.Item1));
            collectionSpecs.Set(JsonHelper.FromJson<CollectionSpec>(database.Item2));
            var collectionConditions = new CollectionCondition.Group();
            collectionConditions.Set(JsonHelper.FromJson<CollectionCondition>(database.Item3));
            var collectionRewards = new CollectionReward.Group();
            collectionRewards.Set(JsonHelper.FromJson<CollectionReward>(database.Item4));
            gameStartStats.Set(JsonHelper.FromJson<GameStartStatsData>(database.Item5));
            var rewardRecords = new List<Contents.Record>();
            foreach (var rewardSpec in collectionSpecs.List)
            {
                var conditions = new List<Stats.Record>();
                if (collectionConditions.TryGetValue(rewardSpec.Id, out var c))
                {
                    conditions.AddRange(c.Select(x => new Stats.Record(x.ConditionName, x.ConditionAmount)));
                }
                var rewards = new List<Stats.Record>();
                if (collectionRewards.TryGetValue(rewardSpec.Id, out var r))
                {
                    rewards.AddRange(r.Select(x => new Stats.Record(x.ConditionName, x.ConditionAmount)));
                }
                var rewardRecord = new Contents.Record(
                    rewardSpec.Id.ToString(),
                    new List<Stats.Record>(),
                    new List<Stats.Record>(),
                    conditions,
                    rewards
                );
                rewardRecords.Add(rewardRecord);
            }
            collections = new Contents(rewardRecords);

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log("End MasterData Update");
        }

        public UniTask BootAsync()
        {
            TinyServiceLocator.Register(this);
            return UniTask.CompletedTask;
        }
#endif

        [Serializable]
        public class Item
        {
            public int Id;

            public string Name;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, Item>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class CollectionSpec
        {
            public int Id;

            public int AcquireItemId;

            public int CoolTimeSeconds;

            public Define.CollectionType CollectionType;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, CollectionSpec>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class CollectionCondition
        {
            public int Id;

            public string ConditionName;

            public int ConditionAmount;

            [Serializable]
            public sealed class Group : Group<int, CollectionCondition>
            {
                public Group() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class CollectionReward
        {
            public int Id;

            public string ConditionName;

            public int ConditionAmount;

            [Serializable]
            public sealed class Group : Group<int, CollectionReward>
            {
                public Group() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class GameStartStatsData
        {
            public int Id;

            public string Name;

            public int Amount;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, GameStartStatsData>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }
    }
}
