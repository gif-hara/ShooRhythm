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

        public static UniTask<Sprite> GetIconAsync(this MasterData.Item self)
        {
            return AssetLoader.LoadAsync<Sprite>($"Textures/Item.{self.Id}");
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
