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
    public static partial class Extensions
    {
        public static Stats.Record ToStatsRecord(this MasterData.StatsData self)
        {
            return new Stats.Record(self.Name, self.Amount);
        }

        public static Stats.Record ToStatsRecordSubtraction(this MasterData.StatsData self)
        {
            return new Stats.Record(self.Name, -self.Amount);
        }

        public static IEnumerable<Stats.Record> ToStatsRecords(this IEnumerable<MasterData.StatsData> self)
        {
            return self.Select(x => x.ToStatsRecord());
        }

        public static IEnumerable<Stats.Record> ToStatsRecordsSubtraction(this IEnumerable<MasterData.StatsData> self)
        {
            return self.Select(x => x.ToStatsRecordSubtraction());
        }

        public static MasterData.Item GetAcquireItem(this MasterData.CollectionSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Items.Get(self.AcquireItemId);
        }

        public static List<MasterData.NeedItem> GetProductionCondition(this MasterData.ProductionSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().ProductionConditions.Get(self.Id);
        }

        public static Contents.Record ToContentsRecord(this MasterData.FishingSpec self)
        {
            return new Contents.Record(
                self.Id.ToString(),
                new List<Stats.Record>(),
                new List<Stats.Record>(),
                new List<Stats.Record>
                {
                    new($"Item.{self.NeedItemId}", self.NeedItemAmount)
                },
                new List<Stats.Record>
                {
                    new($"Item.{self.AcquireItemId}", self.AcquireItemAmount)
                }
            );
        }

        public static Contents.Record ToContentsRecord(this MasterData.MeadowSpec self)
        {
            return new Contents.Record(
                self.Id.ToString(),
                new List<Stats.Record>(),
                new List<Stats.Record>(),
                new List<Stats.Record>
                {
                    new($"Item.{self.NeedItemId}", self.NeedItemAmount)
                },
                new List<Stats.Record>
                {
                    new($"Item.{self.AcquireItemId}", self.AcquireItemAmount),
                    new($"Item.{self.NeedItemId}", -self.NeedItemAmount)
                }
            );
        }

        public static Contents.Record ToContentsRecord(this MasterData.EnemySpec self)
        {
            return new Contents.Record(
                self.Id.ToString(),
                new List<Stats.Record>(),
                new List<Stats.Record>(),
                new List<Stats.Record>(),
                new List<Stats.Record>
                {
                    new($"Item.{self.RewardItemId}", self.RewardItemAmount)
                }
            );
        }

        public static UniTask<Sprite> GetIconAsync(this MasterData.Item self)
        {
            return AssetLoader.LoadAsync<Sprite>($"Textures/Item.{self.Id}");
        }

        public static MasterData.Item GetAcquireItemMasterData(this MasterData.MeadowSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Items.Get(self.AcquireItemId);
        }

        public static MasterData.Item GetAcquireItemMasterData(this MasterData.CollectionSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Items.Get(self.AcquireItemId);
        }

        public static bool IsAllPossession(this IEnumerable<MasterData.NeedItem> self, GameData gameData)
        {
            return self.All(x =>
            {
                if (gameData.Items.TryGetValue(x.NeedItemId, out var amount))
                {
                    return amount.Value >= x.NeedItemAmount;
                }
                return false;
            });
        }
    }
}
