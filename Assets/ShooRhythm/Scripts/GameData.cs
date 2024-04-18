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

        public List<FarmData> FarmDatas { get; } = new();

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

        public void FetchFarmData()
        {
            var diff = Stats.Get("Farm.PlantNumber") - FarmDatas.Count;
            if (diff > 0)
            {
                for (var i = 0; i < diff; i++)
                {
                    FarmDatas.Add(new FarmData());
                }
            }
        }

        public int UserEquipmentItemId => Stats.Get($"UserData.{UserId}.Equipment.ItemId");
    }
}
