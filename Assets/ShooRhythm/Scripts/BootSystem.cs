using System.Linq;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ShooRhythm
{
    /// <summary>
    /// ブートシステム
    /// </summary>
    public sealed class BootSystem : ScriptableObject
    {
        /// <summary>
        /// ブートシステムが初期化完了したか返す
        /// </summary>
        public UniTask IsReady
        {
            get
            {
                return UniTask.WaitUntil(() => initializeState == InitializeState.Initialized);
            }
        }

        /// <summary>
        /// 初期化の状態
        /// </summary>
        private enum InitializeState
        {
            Initializing,
            Initialized,
        }

        private InitializeState initializeState = InitializeState.Initializing;

        void OnEnable()
        {
            TinyServiceLocator.Register(this);
            InitializeInternalAsync().Forget();
        }

        private UniTask InitializeInternalAsync()
        {
            initializeState = InitializeState.Initializing;
            initializeState = InitializeState.Initialized;
            return UniTask.CompletedTask;
        }

#if UNITY_EDITOR
        [MenuItem("Edit/BootSystem/Register")]
        private static void Register()
        {
            if (PlayerSettings.GetPreloadedAssets().Any(x => x is BootSystem))
            {
                Debug.LogWarning("BootSystem is already registered.");
                return;
            }
            var bootSystem = CreateInstance<BootSystem>();
            AssetDatabase.CreateAsset(bootSystem, "Assets/BootSystem.asset");
            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            preloadedAssets.Add(bootSystem);
            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }

    public interface IBootable
    {
        UniTask BootAsync();
    }
}