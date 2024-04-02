using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// ブートシステム
    /// </summary>
    public sealed class BootSystem
    {
        /// <summary>
        /// ブートシステムが初期化完了したか返す
        /// </summary>
        public static UniTask IsReady
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

        private static InitializeState initializeState = InitializeState.Initializing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            InitializeInternalAsync().Forget();
        }

        private static async UniTask InitializeInternalAsync()
        {
            initializeState = InitializeState.Initializing;
            await UniTask.WhenAll
            (
                PlayerSettings.GetPreloadedAssets()
                    .Select(x =>
                    {
                        return x switch
                        {
                            GameObject go => go.TryGetComponent<IBootable>(out var bootable) ? bootable : null,
                            ScriptableObject so => so as IBootable,
                            _ => null,
                        };
                    })
                    .Where(b => b != null)
                    .Select(b => b.BootAsync())
            );
            initializeState = InitializeState.Initialized;
        }
    }

    public interface IBootable
    {
        UniTask BootAsync();
    }
}