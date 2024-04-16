using System;
using System.Collections.Generic;
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
        
        [SerializeField]
        private FishingDesignData seaFishingDesignData;
        public FishingDesignData SeaFishingDesignData => seaFishingDesignData;
        
        [SerializeField]
        private List<FooterData> footers;
        public List<FooterData> Footers => footers;
        
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

        [Serializable]
        public class FooterData
        {
            [SerializeField]
            private string footerName;
            public string FooterName => footerName;
            
            [SerializeField]
            private Define.TabType tabType;
            public Define.TabType TabType => tabType;
            
            [SerializeField]
            private string activeStatsName;
            public string ActiveStatsName => activeStatsName;
        }
    }
}
