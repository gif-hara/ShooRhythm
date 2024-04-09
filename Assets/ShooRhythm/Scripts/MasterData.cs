using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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
        private Reward.DictionaryList rewards;
        public Reward.DictionaryList Rewards => rewards;

        [SerializeField]
        private RewardCondition.Group rewardConditions;
        public RewardCondition.Group RewardConditions => rewardConditions;

#if UNITY_EDITOR
        [ContextMenu("Update")]
        private async void UpdateMasterData()
        {
            Debug.Log("Begin MasterData Update");
            var database = await UniTask.WhenAll(
                GoogleSpreadSheetDownloader.DownloadAsync("Item"),
                GoogleSpreadSheetDownloader.DownloadAsync("Reward"),
                GoogleSpreadSheetDownloader.DownloadAsync("RewardCondition")
            );
            items.Set(JsonHelper.FromJson<Item>(database.Item1));
            rewards.Set(JsonHelper.FromJson<Reward>(database.Item2));
            rewardConditions.Set(JsonHelper.FromJson<RewardCondition>(database.Item3));

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
        public class Reward
        {
            public int Id;

            public int AcquireItemId;

            public int ConditionId;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, Reward>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class RewardCondition
        {
            public int Id;

            public int RequiredItemId;

            public int RequiredItemAmount;

            [Serializable]
            public sealed class Group : Group<int, RewardCondition>
            {
                public Group() : base(x => x.Id) { }
            }
        }
    }
}
