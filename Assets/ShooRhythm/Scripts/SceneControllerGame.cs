using System.Threading;
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
        private HKUIDocument gameItemsDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameCollectionsDocumentPrefab;

        private readonly TinyStateMachine stateMachine = new();

        async UniTask Start()
        {
            await TinyServiceLocator.Resolve<BootSystem>().IsReady;
            var gameData = new GameData();
            TinyServiceLocator.RegisterAsync(gameData).Forget();
            TinyServiceLocator.RegisterAsync(new GameController(destroyCancellationToken)).Forget();
            TinyServiceLocator.RegisterAsync(new GameMessage()).Forget();
            foreach (var i in TinyServiceLocator.Resolve<MasterData>().GameStartStats.List)
            {
                gameData.Stats.Set(i.Name, i.Amount);
            }

            var uiPresenterGameHeader = new UIPresenterGameHeader();
            uiPresenterGameHeader.BeginAsync(gameHeaderDocumentPrefab, destroyCancellationToken).Forget();

            var uiPresenterGameFooter = new UIPresenterGameFooter();
            uiPresenterGameFooter.BeginAsync(gameFooterDocumentPrefab, destroyCancellationToken).Forget();

            stateMachine.Change(StateItems);
        }

        private UniTask StateItems(CancellationToken scope)
        {
            var uiPresenterGameItems = new UIPresenterGameItems();
            uiPresenterGameItems.BeginAsync(gameItemsDocumentPrefab, destroyCancellationToken).Forget();
            return UniTask.CompletedTask;
        }

        private UniTask StateCollections(CancellationToken scope)
        {
            var uiPresenterGameCollection = new UIPresenterGameCollection();
            uiPresenterGameCollection.BeginAsync(gameCollectionsDocumentPrefab, destroyCancellationToken).Forget();
            return UniTask.CompletedTask;
        }
    }
}
