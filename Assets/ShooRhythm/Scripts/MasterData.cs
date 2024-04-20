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
        private NewCollectionSpec.DictionaryList newCollectionSpecs;
        public NewCollectionSpec.DictionaryList NewCollectionSpecs => newCollectionSpecs;

        [SerializeField]
        private ProductionSpec.DictionaryList productionSpecs;
        public ProductionSpec.DictionaryList ProductionSpecs => productionSpecs;

        [SerializeField]
        private StatsData.Group productionConditions;
        public StatsData.Group ProductionConditions => productionConditions;

        [SerializeField]
        private FishingSpec.DictionaryList riverFishingSpecs;
        public FishingSpec.DictionaryList RiverFishingSpecs => riverFishingSpecs;

        [SerializeField]
        private FishingSpec.DictionaryList seaFishingSpecs;
        public FishingSpec.DictionaryList SeaFishingSpecs => seaFishingSpecs;

        [SerializeField]
        private StatsData.DictionaryList grantStatsGameStart;
        public StatsData.DictionaryList GrantStatsGameStart => grantStatsGameStart;

        [SerializeField]
        private Contents questContents;
        public Contents QuestContents => questContents;

        [SerializeField]
        private WeaponSpec.DictionaryList weaponSpecs;
        public WeaponSpec.DictionaryList WeaponSpecs => weaponSpecs;

        [SerializeField]
        private SeedSpec.DictionaryList seedSpecs;
        public SeedSpec.DictionaryList SeedSpecs => seedSpecs;

        [SerializeField]
        private MeadowSpec.DictionaryList meadowSpecs;
        public MeadowSpec.DictionaryList MeadowSpecs => meadowSpecs;

        public UniTask BootAsync()
        {
            TinyServiceLocator.Register(this);
            return UniTask.CompletedTask;
        }

#if UNITY_EDITOR
        [ContextMenu("Update")]
        private async void UpdateMasterData()
        {
            Debug.Log("Begin MasterData Update");
            var database = await UniTask.WhenAll(
                GoogleSpreadSheetDownloader.DownloadAsync("Item"),
                GoogleSpreadSheetDownloader.DownloadAsync("GrantStatsGameStart"),
                GoogleSpreadSheetDownloader.DownloadAsync("QuestSpec"),
                GoogleSpreadSheetDownloader.DownloadAsync("QuestRequired"),
                GoogleSpreadSheetDownloader.DownloadAsync("QuestCondition"),
                GoogleSpreadSheetDownloader.DownloadAsync("QuestIgnore"),
                GoogleSpreadSheetDownloader.DownloadAsync("QuestReward"),
                GoogleSpreadSheetDownloader.DownloadAsync("WeaponSpec"),
                GoogleSpreadSheetDownloader.DownloadAsync("SeedSpec"),
                GoogleSpreadSheetDownloader.DownloadAsync("MeadowSpec"),
                GoogleSpreadSheetDownloader.DownloadAsync("NewCollectionSpec"),
                GoogleSpreadSheetDownloader.DownloadAsync("ProductionSpec"),
                GoogleSpreadSheetDownloader.DownloadAsync("ProductionCondition"),
                GoogleSpreadSheetDownloader.DownloadAsync("RiverFishingSpec"),
                GoogleSpreadSheetDownloader.DownloadAsync("SeaFishingSpec")
            );
            items.Set(JsonHelper.FromJson<Item>(database.Item1));
            grantStatsGameStart.Set(JsonHelper.FromJson<StatsData>(database.Item2));
            var questSpecs = new QuestSpec.DictionaryList();
            questSpecs.Set(JsonHelper.FromJson<QuestSpec>(database.Item3));
            var questRequired = new StatsData.Group();
            questRequired.Set(JsonHelper.FromJson<StatsData>(database.Item4));
            var questConditions = new StatsData.Group();
            questConditions.Set(JsonHelper.FromJson<StatsData>(database.Item5));
            var questIgnores = new StatsData.Group();
            questIgnores.Set(JsonHelper.FromJson<StatsData>(database.Item6));
            var questRewards = new StatsData.Group();
            questRewards.Set(JsonHelper.FromJson<StatsData>(database.Item7));
            weaponSpecs.Set(JsonHelper.FromJson<WeaponSpec>(database.Item8));
            seedSpecs.Set(JsonHelper.FromJson<SeedSpec>(database.Item9));
            meadowSpecs.Set(JsonHelper.FromJson<MeadowSpec>(database.Item10));
            newCollectionSpecs.Set(JsonHelper.FromJson<NewCollectionSpec>(database.Item11));
            productionSpecs.Set(JsonHelper.FromJson<ProductionSpec>(database.Item12));
            productionConditions.Set(JsonHelper.FromJson<StatsData>(database.Item13));
            riverFishingSpecs.Set(JsonHelper.FromJson<FishingSpec>(database.Item14));
            seaFishingSpecs.Set(JsonHelper.FromJson<FishingSpec>(database.Item15));

            var questContentsRecords = new List<Contents.Record>();
            foreach (var questSpec in questSpecs.List)
            {
                var required = new List<Stats.Record>();
                if (questRequired.TryGetValue(questSpec.Id, out var r))
                {
                    required.AddRange(r.Select(x => new Stats.Record(x.Name, x.Amount)));
                }
                var conditions = new List<Stats.Record>();
                if (questConditions.TryGetValue(questSpec.Id, out var c))
                {
                    conditions.AddRange(c.Select(x => new Stats.Record(x.Name, x.Amount)));
                }
                var ignores = new List<Stats.Record>();
                if (questIgnores.TryGetValue(questSpec.Id, out var i))
                {
                    ignores.AddRange(i.Select(x => new Stats.Record(x.Name, x.Amount)));
                }
                var rewards = new List<Stats.Record>();
                if (questRewards.TryGetValue(questSpec.Id, out var re))
                {
                    rewards.AddRange(re.Select(x => new Stats.Record(x.Name, x.Amount)));
                }
                var record = new Contents.Record(
                    questSpec.Id.ToString(),
                    required,
                    ignores,
                    conditions,
                    rewards
                );
                questContentsRecords.Add(record);
            }
            questContents = new Contents(questContentsRecords);

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
        public class NewCollectionSpec
        {
            public int Id;

            public int AcquireItemId;

            public int AcquireItemAmount;

            public int CoolTimeSeconds;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, NewCollectionSpec>
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
        public class ProductionSpec
        {
            public int Id;

            public int AcquireItemId;

            public int AcquireItemAmount;

            public int CoolTimeSeconds;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, ProductionSpec>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class FishingSpec
        {
            public int Id;

            public int AcquireItemId;

            public int AcquireItemAmount;

            public int NeedItemId;

            public int NeedItemAmount;

            public float CoolTimeSeconds;

            public float WaitSecondsMin;

            public float WaitSecondsMax;

            public float PostponementSeconds;

            [Serializable]
            public class DictionaryList : DictionaryList<int, FishingSpec>
            {
                public DictionaryList() : base(x => x.Id) { }
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

        [Serializable]
        public class QuestSpec
        {
            public int Id;

            public class DictionaryList : DictionaryList<int, QuestSpec>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class StatsData
        {
            public int Id;

            public string Name;

            public int Amount;

            public StatsData(string name, int amount)
            {
                Name = name;
                Amount = amount;
            }

            [Serializable]
            public class Group : Group<int, StatsData>
            {
                public Group() : base(x => x.Id) { }
            }

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, StatsData>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class WeaponSpec
        {
            public int Id;

            public int ItemId;

            public int Strength;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, WeaponSpec>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class SeedSpec
        {
            public int Id;

            public int SeedItemId;

            public int AcquireItemId;

            public float GrowSeconds;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, SeedSpec>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class MeadowSpec
        {
            public int Id;

            public int NeedItemId;

            public int NeedItemAmount;

            public int AcquireItemId;

            public int AcquireItemAmount;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, MeadowSpec>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }
    }
}
