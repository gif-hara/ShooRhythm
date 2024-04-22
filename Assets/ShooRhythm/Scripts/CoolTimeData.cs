using System.Threading;
using R3;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CoolTimeData
    {
        public ReactiveProperty<float> CoolTime { get; private set; } = new();

        public float Max { get; private set; }

        public void Set(float max)
        {
            Max = max;
            CoolTime.Value = max;
        }
    }
}
