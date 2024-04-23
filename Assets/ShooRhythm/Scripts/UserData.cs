using System;
using System.Collections.Generic;
using System.Threading;
using R3;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class UserData
    {
        public int id;

        public readonly ReactiveProperty<int> equipmentItemId = new();

        public readonly List<CoolTimeData> coolTimeData = new();

        private readonly CancellationTokenSource cancellationTokenSource = new();

        public UserData(int initialCoolTimeNumber)
        {
            for (var i = 0; i < initialCoolTimeNumber; i++)
            {
                coolTimeData.Add(new CoolTimeData());
            }
        }

        ~UserData()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        public void SetCoolTime(int index, float value)
        {
            coolTimeData[index].Set(value);
        }

        public int GetAvailableCoolTimeIndex()
        {
            for (var i = 0; i < coolTimeData.Count; i++)
            {
                if (coolTimeData[i].CoolTime.Value <= 0)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
