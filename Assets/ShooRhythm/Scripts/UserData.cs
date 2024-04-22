using System;
using R3;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class UserData
    {
        public int id;

        public readonly ReactiveProperty<int> equipmentItemId = new ();
    }
}
