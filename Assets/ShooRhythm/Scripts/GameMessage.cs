using R3;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameMessage
    {
        public readonly Subject<(int id, int count)> UpdatedItem = new();

        public readonly Subject<Define.TabType> RequestChangeTab = new();

        public readonly Subject<(string message, Sprite sprite)> RequestNotification = new();
    }
}
