using System.Collections.Generic;
using HK;
using SCD;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameData
    {
        public Stats Stats { get; } = new Stats();

        private readonly Dictionary<int, int> items = new();

        public void SetItem(int id, int count)
        {
            items[id] = count;
            TinyServiceLocator.Resolve<GameMessage>().UpdatedItem.OnNext((id, count));
        }

        public int GetItem(int id)
        {
            return items.TryGetValue(id, out var count) ? count : 0;
        }
    }
}
