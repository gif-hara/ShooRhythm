using System;
using UnityEngine;
using UnityEngine.Serialization;

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
            /// <summary>
            /// ヒット状態になるまでの最小待機時間（秒）
            /// </summary>
            [SerializeField]
            private float waitSecondsMin;
            public float WaitSecondsMin => waitSecondsMin;
            
            /// <summary>
            /// ヒット状態になるまでの最大待機時間（秒）
            /// </summary>
            [SerializeField]
            private float waitSecondsMax;
            public float WaitSecondsMax => waitSecondsMax;
            
            /// <summary>
            /// ヒットしてから釣り上げられる猶予（秒）
            /// </summary>
            [SerializeField]
            private float postponementSeconds;
            public float PostponementSeconds => postponementSeconds;

            /// <summary>
            /// 獲得できる収集Specの名前
            /// </summary>
            [SerializeField]
            private string acquireCollectionSpecName;
            public string AcquireCollectionSpecName => acquireCollectionSpecName;
        }
    }
}
