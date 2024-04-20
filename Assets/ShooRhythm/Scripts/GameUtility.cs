using System.Collections.Generic;
using HK;
using SCD;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameUtility
    {
        public static void ShowContentsConditionsNotification(Contents.Record contentsRecord)
        {
            var gameMessage = TinyServiceLocator.Resolve<GameMessage>();
            foreach (var i in contentsRecord.Conditions)
            {
                if (i.Name.StartsWith("Item.", System.StringComparison.Ordinal))
                {
                    var itemId = int.Parse(i.Name.Substring(5));
                    var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(itemId);
                    gameMessage.RequestNotification.OnNext(
                        (
                            $"{masterDataItem.Name}が{i.Value}個必要です",
                            null,
                            Define.NotificationType.Negative
                        )
                    );
                }
                else
                {
                    Debug.LogWarning($"Unknown record name: {i.Name}");
                }
            }
        }
    }
}
