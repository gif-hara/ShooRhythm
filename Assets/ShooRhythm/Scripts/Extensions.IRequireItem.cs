using HK;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static void ShowRequireItemNotification(this IRequireItem self)
        {
            GameUtility.ShowRequireItemNotification(self.NeedItemId, self.NeedItemAmount);
        }

        public static void ShowRequireItemNotification(this IRequireItemList self)
        {
            GameUtility.ShowRequireItemNotification(self.NeedItems);
        }

        public static bool HasItems(this IRequireItem self)
        {
            return TinyServiceLocator.Resolve<GameData>().GetItem(self.NeedItemId) >= self.NeedItemAmount;
        }

        public static bool HasItems(this IRequireItemList self)
        {
            var gameData = TinyServiceLocator.Resolve<GameData>();
            foreach (var (itemId, amount) in self.NeedItems)
            {
                if (gameData.GetItem(itemId) < amount)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
