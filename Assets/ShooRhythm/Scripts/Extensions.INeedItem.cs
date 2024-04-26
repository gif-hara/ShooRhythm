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

        public static bool HasItems(this INeedItem self)
        {
            return TinyServiceLocator.Resolve<GameData>().GetItem(self.NeedItemId) >= self.NeedItemAmount;
        }
    }
}
