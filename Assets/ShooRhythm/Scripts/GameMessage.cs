using R3;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameMessage
    {
        public readonly Subject<Define.TabType> RequestChangeTab = new();

        public readonly Subject<(string message, Sprite sprite, Define.NotificationType notificationType)> RequestNotification = new();
        
        public readonly Subject<int> AddedItem = new();
        
        public readonly Subject<Unit> AddedProductMachineData = new();
    }
}
