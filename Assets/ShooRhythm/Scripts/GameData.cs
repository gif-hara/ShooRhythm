using System.Collections.Generic;
using HK;
using R3;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameData
    {
        public HashSet<string> AvailableContents { get; } = new();

        public Dictionary<int, ReactiveProperty<int>> Items { get; } = new();

        public List<FarmData> FarmDatas { get; } = new();

        public Dictionary<Define.DungeonType, EnemyInstanceData> DungeonEnemyInstanceDatas { get; } = new();

        public Dictionary<int, UserData> UserData { get; } = new();

        public int CurrentUserId { get; set; } = 0;

        public UserData CurrentUserData => UserData[CurrentUserId];

        public List<ProductMachineData> ProductMachineData { get; } = new();

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
    }
}
