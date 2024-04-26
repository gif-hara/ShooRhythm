using System.Collections.Generic;

namespace ShooRhythm
{
    /// <summary>
    /// アイテムを要求するインターフェース
    /// </summary>
    public interface INeedItem
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
}
