using System.Linq;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;
using System.Collections.Generic;
using UnitySequencerSystem;
using System;
using System.Threading;
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
        [SerializeReference, SubclassSelector]
        private List<ISequence> sequences = default;

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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var bootSystem = Resources.Load<BootSystem>("BootSystem");
            TinyServiceLocator.Register(bootSystem);
            bootSystem.InitializeInternalAsync().Forget();
        }

        private async UniTask InitializeInternalAsync()
        {
            initializeState = InitializeState.Initializing;
            await new Sequencer(new Container(), sequences).PlayAsync(Application.exitCancellationToken);
            initializeState = InitializeState.Initialized;
        }

#if UNITY_EDITOR
        [MenuItem("Edit/BootSystem/Register")]
        private static void Register()
        {
            if (AssetDatabase.LoadAssetAtPath<BootSystem>("Assets/Resources/BootSystem.asset") != null)
            {
                Debug.LogWarning("BootSystem is already registered.");
                return;
            }
            var bootSystem = CreateInstance<BootSystem>();
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            AssetDatabase.CreateAsset(bootSystem, "Assets/Resources/BootSystem.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }

    [Serializable]
    public class InitializeBootGameObject : ISequence
    {
        [SerializeField]
        private List<GameObject> bootObjects = default;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            foreach (var bootObject in bootObjects)
            {
                var instance = UnityEngine.Object.Instantiate(bootObject);
                UnityEngine.Object.DontDestroyOnLoad(instance);
                if (instance.TryGetComponent<IBootable>(out var bootable))
                {
                    await bootable.BootAsync();
                }
                else
                {
                    Debug.LogWarning($"{instance.name} is not IBootable.");
                }
            }
        }
    }

    [Serializable]
    public class InitializeBootScriptableObject : ISequence
    {
        [SerializeField]
        private List<ScriptableObject> bootObjects = default;

        public async UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            foreach (var bootObject in bootObjects)
            {
                if (bootObject is IBootable bootable)
                {
                    await bootable.BootAsync();
                }
                else
                {
                    Debug.LogWarning($"{bootObject.name} is not IBootable.");
                }
            }
        }
    }

    public interface IBootable
    {
        UniTask BootAsync();
    }
}