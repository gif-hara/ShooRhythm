using System.Collections.Generic;
using HK;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static void ShowRequireItemNotification(this INeedItem self)
        {
            var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(self.NeedItemId);
            var gameMessage = TinyServiceLocator.Resolve<GameMessage>();
            gameMessage.RequestNotification.OnNext(
                (
                    $"{masterDataItem.Name}が{self.NeedItemAmount}個必要です",
                    null,
                    Define.NotificationType.Negative
                )
            );
        }
        
        public static void ShowRequireItemNotification(this IEnumerable<INeedItem> self)
        {
            foreach (var needItem in self)
            {
                if (needItem.HasItems())
                {
                    continue;
                }
                needItem.ShowRequireItemNotification();
            }
        }

        public static bool HasItems(this INeedItem self)
        {
            return TinyServiceLocator.Resolve<GameData>().GetItem(self.NeedItemId) >= self.NeedItemAmount;
        }
        
        public static bool HasItems(this IEnumerable<INeedItem> self)
        {
            foreach (var needItem in self)
            {
                if (!needItem.HasItems())
                {
                    return false;
                }
            }
            return true;
        }
        
        public static MasterData.Item GetMasterDataItem(this INeedItem self)
        {
            return TinyServiceLocator.Resolve<MasterData>().Items.Get(self.NeedItemId);
        }
    }
}
