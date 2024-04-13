using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using SCD;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "MasterData", menuName = "ShooRhythm/MasterData")]
    public sealed class MasterData : ScriptableObject
    {
        [SerializeField]
        private Item.DictionaryList items;
        public Item.DictionaryList Items => items;

        [SerializeField]
        private RewardSpec.DictionaryList rewardSpecs;
        public RewardSpec.DictionaryList RewardSpecs => rewardSpecs;

        [SerializeField]
        private RewardCondition.Group rewardConditions;
        public RewardCondition.Group RewardConditions => rewardConditions;

        [SerializeField]
        private Contents rewards;

#if UNITY_EDITOR
        [ContextMenu("Update")]
        private async void UpdateMasterData()
        {
            Debug.Log("Begin MasterData Update");
            var database = await UniTask.WhenAll(
                GoogleSpreadSheetDownloader.DownloadAsync("Item"),
                GoogleSpreadSheetDownloader.DownloadAsync("RewardSpec"),
                GoogleSpreadSheetDownloader.DownloadAsync("RewardCondition")
            );
            items.Set(JsonHelper.FromJson<Item>(database.Item1));
            rewardSpecs.Set(JsonHelper.FromJson<RewardSpec>(database.Item2));
            rewardConditions.Set(JsonHelper.FromJson<RewardCondition>(database.Item3));
            var rewardRecords = new List<Contents.Record>();
            foreach (var rewardSpec in rewardSpecs.List)
            {
                var conditions = new List<Stats.Record>();
                if (rewardConditions.TryGetValue(rewardSpec.Id, out var c))
                {
                    conditions.AddRange(c.Select(x => new Stats.Record(x.ConditionName, x.ConditionAmount)));
                }
                var rewardRecord = new Contents.Record(
                    rewardSpec.Id.ToString(),
                    new List<Stats.Record>(),
                    new List<Stats.Record>(),
                    conditions,
                    new List<Stats.Record>
                    {
                        new($"Item.{rewardSpec.AcquireItemId}", 1)
                    });
                rewardRecords.Add(rewardRecord);
            }
            rewards = new Contents(rewardRecords);

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log("End MasterData Update");
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
        public class RewardSpec
        {
            public int Id;

            public int AcquireItemId;

            public int CoolTimeSeconds;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, RewardSpec>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class RewardCondition
        {
            public int Id;

            public string ConditionName;

            public int ConditionAmount;

            [Serializable]
            public sealed class Group : Group<int, RewardCondition>
            {
                public Group() : base(x => x.Id) { }
            }
        }
    }
}
