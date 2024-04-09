using System.Collections.Generic;
using UnityEngine.Assertions;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameData
    {
        private readonly Dictionary<int, int> itemNumbers = new();

        public void AddItem(int itemId, int number)
        {
            if (itemNumbers.TryGetValue(itemId, out var currentNumber))
            {
                itemNumbers[itemId] = currentNumber + number;
            }
            else
            {
                itemNumbers[itemId] = number;
            }

            Assert.IsTrue(itemNumbers[itemId] >= 0);
        }

        public int GetItemNumber(int itemId)
        {
            return itemNumbers.TryGetValue(itemId, out var number) ? number : 0;
        }
    }
}
