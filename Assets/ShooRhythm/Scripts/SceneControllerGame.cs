using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SceneControllerGame : MonoBehaviour
    {
        [SerializeField]
        private HKUIDocument gameIndexDocumentPrefab;

        async UniTask Start()
        {
            await TinyServiceLocator.Resolve<BootSystem>().IsReady;
            TinyServiceLocator.RegisterAsync(new GameData()).Forget();

            var uiPresenterGameIndex = new UIPresenterGameIndex();
            uiPresenterGameIndex.BeginAsync(gameIndexDocumentPrefab, destroyCancellationToken).Forget();
        }
    }
}
