using System;
using System.Collections.Generic;
using R3;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class ProductMachineData
    {
        public readonly ReactiveProperty<int> productItemId = new ();
        
        public readonly List<ReactiveProperty<int>> slotItemIds = new ();
        
        public ProductMachineData()
        {
            for (var i = 0; i < Define.MachineSlotCount; i++)
            {
                slotItemIds.Add(new ReactiveProperty<int>());
            }
        }
    }
}
