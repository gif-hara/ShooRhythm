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
        public Stats Stats { get; } = new();

        public Dictionary<int, int> Items { get; } = new();

        public int UserId { get; set; } = 0;

        public void SetItem(int id, int count)
        {
            Items[id] = count;
            TinyServiceLocator.Resolve<GameMessage>().UpdatedItem.OnNext((id, count));
        }

        public int GetItem(int id)
        {
            return Items.TryGetValue(id, out var count) ? count : 0;
        }

        public int UserEquipmentItemId
        {
            get => Stats.Get($"UserData.{UserId}.Equipment.ItemId");
            set => Stats.Set($"UserData.{UserId}.Equipment.ItemId", value);
        }
    }
}
