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

        [SerializeField]
        private HKUIDocument gameCollectionDocumentPrefab;

        async UniTask Start()
        {
            await TinyServiceLocator.Resolve<BootSystem>().IsReady;
            TinyServiceLocator.RegisterAsync(new GameData()).Forget();
            TinyServiceLocator.RegisterAsync(new GameController()).Forget();

            var uiPresenterGameIndex = new UIPresenterGameFooter();
            uiPresenterGameIndex.BeginAsync(gameIndexDocumentPrefab, destroyCancellationToken).Forget();

            var uiPresenterGameCollection = new UIPresenterGameCollection();
            uiPresenterGameCollection.BeginAsync(gameCollectionDocumentPrefab, destroyCancellationToken).Forget();
        }
    }
}
