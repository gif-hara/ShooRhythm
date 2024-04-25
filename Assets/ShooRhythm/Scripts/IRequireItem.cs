using System.Collections.Generic;

namespace ShooRhythm
{
    /// <summary>
    /// アイテムを要求するインターフェース
    /// </summary>
    public interface IRequireItem
    {
        /// <summary>
        /// 必要なアイテムID
        /// </summary>
        int NeedItemId { get; }

        /// <summary>
        /// 必要なアイテム数
        /// </summary>
        int NeedItemAmount { get; }
    }

    public interface IRequireItemList
    {
        /// <summary>
        /// 必要なアイテムIDと数のリスト
        /// </summary>
        IEnumerable<(int itemId, int amount)> NeedItems { get; }
    }
}
