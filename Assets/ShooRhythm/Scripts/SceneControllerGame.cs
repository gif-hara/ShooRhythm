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
        private HKUIDocument gameHeaderDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameFooterDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameCollectionDocumentPrefab;

        async UniTask Start()
        {
            await TinyServiceLocator.Resolve<BootSystem>().IsReady;
            TinyServiceLocator.RegisterAsync(new GameData()).Forget();
            TinyServiceLocator.RegisterAsync(new GameController()).Forget();

            var uiPresenterGameHeader = new UIPresenterGameHeader();
            uiPresenterGameHeader.BeginAsync(gameHeaderDocumentPrefab, destroyCancellationToken).Forget();

            var uiPresenterGameFooter = new UIPresenterGameFooter();
            uiPresenterGameFooter.BeginAsync(gameFooterDocumentPrefab, destroyCancellationToken).Forget();

            var uiPresenterGameCollection = new UIPresenterGameCollection();
            uiPresenterGameCollection.BeginAsync(gameCollectionDocumentPrefab, destroyCancellationToken).Forget();
        }
    }
}
