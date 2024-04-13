using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
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
            var gameController = new GameController(destroyCancellationToken);
            TinyServiceLocator.RegisterAsync(gameController).Forget();
            var gameMessage = new GameMessage();
            TinyServiceLocator.RegisterAsync(gameMessage).Forget();

            gameMessage.RequestChangeTab
                .Subscribe(x =>
                {
                    switch (x)
                    {
                        case Define.TabType.Items:
                            stateMachine.Change(StateItems);
                            break;
                        case Define.TabType.Collections:
                            stateMachine.Change(StateCollections);
                            break;
                    }
                })
                .RegisterTo(destroyCancellationToken);

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
            uiPresenterGameItems.BeginAsync(gameItemsDocumentPrefab, scope).Forget();
            return UniTask.CompletedTask;
        }

        private UniTask StateCollections(CancellationToken scope)
        {
            var uiPresenterGameCollections = new UIPresenterGameCollections();
            uiPresenterGameCollections.BeginAsync(gameCollectionsDocumentPrefab, scope).Forget();
            return UniTask.CompletedTask;
        }
    }
}
