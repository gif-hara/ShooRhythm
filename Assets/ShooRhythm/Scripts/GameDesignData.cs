using System;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(menuName = "ShooRhythm/GameDesignData")]
    public sealed class GameDesignData : ScriptableObject
    {
        [SerializeField]
        private FishingDesignData riverFishingDesignData;
        public FishingDesignData RiverFishingDesignData => riverFishingDesignData;
        
        [Serializable]
        public class FishingDesignData
        {
            [SerializeField]
            private float waitSecondsMin;
            public float WaitSecondsMin => waitSecondsMin;
            
            [SerializeField]
            private float waitSecondsMax;
            public float WaitSecondsMax => waitSecondsMax;
        }
    }
}
