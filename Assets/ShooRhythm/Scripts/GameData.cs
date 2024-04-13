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

        public Dictionary<int, int> Items { get; } = new Dictionary<int, int>();

        public void SetItem(int id, int count)
        {
            Items[id] = count;
            TinyServiceLocator.Resolve<GameMessage>().UpdatedItem.OnNext((id, count));
        }

        public int GetItem(int id)
        {
            return Items.TryGetValue(id, out var count) ? count : 0;
        }
    }
}
