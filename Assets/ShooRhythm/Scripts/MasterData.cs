using System;
using Cysharp.Threading.Tasks;
using HK;
using UnityEditor;
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
        private ProductionSpec.DictionaryList productionSpecs;
        public ProductionSpec.DictionaryList ProductionSpecs => productionSpecs;

        [SerializeField]
        private NeedItem.Group productionConditions;
        public NeedItem.Group ProductionConditions => productionConditions;

        [SerializeField]
        private FishingSpec.DictionaryList riverFishingSpecs;
        public FishingSpec.DictionaryList RiverFishingSpecs => riverFishingSpecs;

        [SerializeField]
        private FishingSpec.DictionaryList seaFishingSpecs;
        public FishingSpec.DictionaryList SeaFishingSpecs => seaFishingSpecs;

        [SerializeField]
        private AvailableContent.DictionaryList grantStatsGameStart;
        public AvailableContent.DictionaryList GrantStatsGameStart => grantStatsGameStart;

        [SerializeField]
        private QuestSpec.DictionaryList questSpecs;
        public QuestSpec.DictionaryList QuestSpecs => questSpecs;

        [SerializeField]
        private AvailableContent.Group questRequires;
        public AvailableContent.Group QuestRequires => questRequires;

        [SerializeField]
        private AvailableContent.Group questIgnores;
        public AvailableContent.Group QuestIgnores => questIgnores;

        [SerializeField]
        private NeedItem.Group questConditions;
        public NeedItem.Group QuestConditions => questConditions;

        [SerializeField]
        private AvailableContent.Group questRewards;
        public AvailableContent.Group QuestRewards => questRewards;

        [SerializeField]
        private WeaponSpec.DictionaryList weaponSpecs;
        public WeaponSpec.DictionaryList WeaponSpecs => weaponSpecs;

        [SerializeField]
        private SeedSpec.DictionaryList seedSpecs;
        public SeedSpec.DictionaryList SeedSpecs => seedSpecs;

        [SerializeField]
        private MeadowSpec.DictionaryList meadowSpecs;
        public MeadowSpec.DictionaryList MeadowSpecs => meadowSpecs;

        [SerializeField]
        private EnemySpec.Group enemySpecs;
        public EnemySpec.Group EnemySpecs => enemySpecs;

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
                GoogleSpreadSheetDownloader.DownloadAsync("CollectionSpec"),
                GoogleSpreadSheetDownloader.DownloadAsync("ProductionSpec"),
                GoogleSpreadSheetDownloader.DownloadAsync("ProductionCondition"),
                GoogleSpreadSheetDownloader.DownloadAsync("RiverFishingSpec"),
                GoogleSpreadSheetDownloader.DownloadAsync("SeaFishingSpec"),
                GoogleSpreadSheetDownloader.DownloadAsync("EnemySpec")
            );
            items.Set(JsonHelper.FromJson<Item>(database[0]));
            foreach (var i in items.List)
            {
                i.Icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/ShooRhythm/Textures/Item.{i.Id}.png");
            }
            grantStatsGameStart.Set(JsonHelper.FromJson<AvailableContent>(database[1]));
            questSpecs.Set(JsonHelper.FromJson<QuestSpec>(database[2]));
            questRequires.Set(JsonHelper.FromJson<AvailableContent>(database[3]));
            questConditions.Set(JsonHelper.FromJson<NeedItem>(database[4]));
            questIgnores.Set(JsonHelper.FromJson<AvailableContent>(database[5]));
            questRewards.Set(JsonHelper.FromJson<AvailableContent>(database[6]));
            weaponSpecs.Set(JsonHelper.FromJson<WeaponSpec>(database[7]));
            seedSpecs.Set(JsonHelper.FromJson<SeedSpec>(database[8]));
            meadowSpecs.Set(JsonHelper.FromJson<MeadowSpec>(database[9]));
            collectionSpecs.Set(JsonHelper.FromJson<CollectionSpec>(database[10]));
            productionSpecs.Set(JsonHelper.FromJson<ProductionSpec>(database[11]));
            productionConditions.Set(JsonHelper.FromJson<NeedItem>(database[12]));
            riverFishingSpecs.Set(JsonHelper.FromJson<FishingSpec>(database[13]));
            seaFishingSpecs.Set(JsonHelper.FromJson<FishingSpec>(database[14]));
            enemySpecs.Set(JsonHelper.FromJson<EnemySpec>(database[15]));

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

            public Sprite Icon;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, Item>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class NeedItem : INeedItem
        {
            public int Id;

            public int NeedItemId;

            public int NeedItemAmount;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, NeedItem>
            {
                public DictionaryList() : base(x => x.Id) { }
            }

            [Serializable]
            public sealed class Group : Group<int, NeedItem>
            {
                public Group() : base(x => x.Id) { }
            }

            int INeedItem.NeedItemId => NeedItemId;

            int INeedItem.NeedItemAmount => NeedItemAmount;
        }

        [Serializable]
        public class AvailableContent
        {
            public int Id;

            public string Name;

            [Serializable]
            public class DictionaryList : DictionaryList<int, AvailableContent>
            {
                public DictionaryList() : base(x => x.Id) { }
            }

            [Serializable]
            public class Group : Group<int, AvailableContent>
            {
                public Group() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class CollectionSpec
        {
            public int Id;

            public int AcquireItemId;

            public int AcquireItemAmount;

            public int CoolTimeSeconds;

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
        public class FishingSpec : INeedItem
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

            int INeedItem.NeedItemId => NeedItemId;

            int INeedItem.NeedItemAmount => NeedItemAmount;

            [Serializable]
            public class DictionaryList : DictionaryList<int, FishingSpec>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class QuestSpec
        {
            public int Id;

            [Serializable]
            public class DictionaryList : DictionaryList<int, QuestSpec>
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
        public class SeedSpec : INeedItem
        {
            public int Id;

            public int SeedItemId;

            public int AcquireItemId;

            public float GrowSeconds;

            public int NeedItemId => SeedItemId;

            public int NeedItemAmount => 1;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, SeedSpec>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class MeadowSpec : INeedItem
        {
            public int Id;

            public int NeedItemId;

            public int NeedItemAmount;

            public int AcquireItemId;

            public int AcquireItemAmount;

            int INeedItem.NeedItemId => NeedItemId;

            int INeedItem.NeedItemAmount => NeedItemAmount;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, MeadowSpec>
            {
                public DictionaryList() : base(x => x.Id) { }
            }
        }

        [Serializable]
        public class EnemySpec
        {
            public int Id;

            public string Name;

            public Define.DungeonType DungeonType;

            public int HitPoint;

            public int RewardItemId;

            public int RewardItemAmount;

            [Serializable]
            public sealed class DictionaryList : DictionaryList<int, EnemySpec>
            {
                public DictionaryList() : base(x => x.Id) { }
            }

            [Serializable]
            public sealed class Group : Group<Define.DungeonType, EnemySpec>
            {
                public Group() : base(x => x.DungeonType) { }
            }
        }
    }
}
