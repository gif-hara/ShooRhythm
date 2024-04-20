using System.Collections.Generic;
using HK;
using SCD;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
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
