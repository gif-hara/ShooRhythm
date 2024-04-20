using System.Collections.Generic;
using System.Linq;
using HK;
using SCD;

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

        public static MasterData.Item GetItem(this MasterData.CollectionSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Items.Get(self.Id);
        }

        public static MasterData.Item GetAcquireItem(this MasterData.NewCollectionSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Items.Get(self.AcquireItemId);
        }

        public static Contents.Record ToContentsRecord(this MasterData.NewCollectionSpec self)
        {
            return new Contents.Record(
                self.Id.ToString(),
                new List<Stats.Record>(),
                new List<Stats.Record>(),
                new List<Stats.Record>(),
                new List<Stats.Record> { new($"Item.{self.AcquireItemId}", self.AcquireItemAmount) }
            );
        }

        public static List<MasterData.StatsData> GetProductionCondition(this MasterData.ProductionSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().ProductionConditions.Get(self.Id);
        }

        public static Contents.Record ToContentsRecord(this MasterData.ProductionSpec self)
        {
            var rewards = new List<Stats.Record>(self.GetProductionCondition().ToStatsRecordsSubtraction());
            rewards.Add(new($"Item.{self.AcquireItemId}", self.AcquireItemAmount));
            return new Contents.Record(
                self.Id.ToString(),
                new List<Stats.Record>(),
                new List<Stats.Record>(),
                self.GetProductionCondition().ToStatsRecords().ToList(),
                rewards
            );
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
    }
}
