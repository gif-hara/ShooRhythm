using System;
using HK;
using R3;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class FarmData
    {
        public ReactiveProperty<int> SeedItemId { get; } = new();

        public ReactiveProperty<long> PlantTicks { get; } = new();

        public MasterData.SeedSpec SeedSpec => TinyServiceLocator.Resolve<MasterData>().SeedSpecs.Get(SeedItemId.Value);

        public bool IsCompleted
        {
            get
            {
                if (SeedItemId.Value == 0 || PlantTicks.Value == 0)
                {
                    return false;
                }
                return DateTime.UtcNow.Ticks >= new DateTime(PlantTicks.Value).AddSeconds(SeedSpec.GrowSeconds).Ticks;
            }
        }
    }
}
