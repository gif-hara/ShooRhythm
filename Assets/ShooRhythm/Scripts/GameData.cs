using System.Collections.Generic;
using HK;
using R3;
using SCD;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameData
    {
        public Stats Stats { get; } = new();

        public Dictionary<int, ReactiveProperty<int>> Items { get; } = new();

        public List<FarmData> FarmDatas { get; } = new();

        public Dictionary<Define.DungeonType, EnemyInstanceData> DungeonEnemyInstanceDatas { get; } = new();

        public int UserId { get; set; } = 0;

        public void SetItem(int id, int count)
        {
            if (!Items.ContainsKey(id))
            {
                Items.Add(id, new ReactiveProperty<int>(count));
                TinyServiceLocator.Resolve<GameMessage>().AddedItem.OnNext(id);
            }
            else
            {
                Items[id].Value = count;
            }
        }

        public int GetItem(int id)
        {
            return Items.TryGetValue(id, out var reactiveProperty) ? reactiveProperty.Value : 0;
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
