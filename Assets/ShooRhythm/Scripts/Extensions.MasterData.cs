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
        public static MasterData.Item GetAcquireItem(this MasterData.CollectionSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Items.Get(self.AcquireItemId);
        }

        public static List<MasterData.NeedItem> GetProductionNeedItems(this MasterData.ProductionSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().ProductionConditions.Get(self.Id);
        }
        
        public static MasterData.Item GetAcquireItemMasterData(this MasterData.CollectionSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Items.Get(self.AcquireItemId);
        }
        
        public static bool IsAvailable(this MasterData.AvailableContent self)
        {
            return TinyServiceLocator.Resolve<GameData>().AvailableContents.Contains(self.Name);
        }
        
        public static bool IsAvailableAny(this IEnumerable<MasterData.AvailableContent> self)
        {
            return self.Any(x => x.IsAvailable());
        }
        
        public static bool IsAvailableAll(this IEnumerable<MasterData.AvailableContent> self)
        {
            return self.All(x => x.IsAvailable());
        }
        
        public static List<MasterData.NeedItem> GetQuestConditions(this MasterData.QuestSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().QuestConditions.Get(self.Id);
        }
        
        public static List<MasterData.AvailableContent> GetQuestRewards(this MasterData.QuestSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().QuestRewards.Get(self.Id);
        }
        
        public static List<MasterData.AvailableContent> GetQuestIgnores(this MasterData.QuestSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().QuestIgnores.Get(self.Id);
        }
        
        public static List<MasterData.AvailableContent> GetQuestRequires(this MasterData.QuestSpec self)
        {
            return TinyServiceLocator.Resolve<MasterData>().QuestRequires.Get(self.Id);
        }
    }
}
