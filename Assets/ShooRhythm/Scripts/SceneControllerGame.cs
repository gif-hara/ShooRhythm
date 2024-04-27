using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SceneControllerGame : MonoBehaviour
    {
        [SerializeField]
        private GameDesignData gameDesignData;

        [SerializeField]
        private HKUIDocument gameNotificationDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameFooterDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameItemsDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameCollectionsDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameProductionsDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameQuestsDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameEquipmentDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameRiverFishingDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameSeaFishingDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameFarmDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameMeadowDocumentPrefab;

        [SerializeField]
        private HKUIDocument gameBattleDocumentPrefab;

        private readonly TinyStateMachine stateMachine = new();

        async UniTask Start()
        {
            await TinyServiceLocator.Resolve<BootSystem>().IsReady;
            TinyServiceLocator.RegisterAsync(gameDesignData).Forget();
            var gameData = new GameData();
            TinyServiceLocator.RegisterAsync(gameData).Forget();
            var gameController = new GameController(0, destroyCancellationToken);
            TinyServiceLocator.RegisterAsync(gameController).Forget();
            var gameMessage = new GameMessage();
            TinyServiceLocator.RegisterAsync(gameMessage).Forget();
#if DEBUG
            GameDebugController.Begin(destroyCancellationToken);
            TinyServiceLocator.Register(new GameDebugData());
#endif

            for (var i = 0; i < gameDesignData.InitialProductMachineNumber; i++)
            {
                gameController.AddProductMachine();
            }
            for (var i = 0; i < gameDesignData.InitialFarmNumber; i++)
            {
                gameController.AddFarmData();
            }

            await UniTask.WhenAll(
                TinyServiceLocator.Resolve<MasterData>().GrantStatsGameStart.List
                    .Select(x => gameController.AddAvailableContentAsync(x.Name))
            );

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
                        case Define.TabType.Productions:
                            stateMachine.Change(StateProductions);
                            break;
                        case Define.TabType.Quests:
                            stateMachine.Change(StateQuests);
                            break;
                        case Define.TabType.Equipment:
                            stateMachine.Change(StateEquipment);
                            break;
                        case Define.TabType.RiverFishing:
                            stateMachine.Change(StateRiverFishing);
                            break;
                        case Define.TabType.SeaFishing:
                            stateMachine.Change(StateSeaFishing);
                            break;
                        case Define.TabType.Farm:
                            stateMachine.Change(StateFarm);
                            break;
                        case Define.TabType.Meadow:
                            stateMachine.Change(StateMeadow);
                            break;
                        case Define.TabType.Grassland:
                            stateMachine.Change(StateGrassland);
                            break;
                    }
                })
                .RegisterTo(destroyCancellationToken);

            var uiPresenterGameFooter = new UIPresenterGameFooter();
            uiPresenterGameFooter.BeginAsync(gameFooterDocumentPrefab, destroyCancellationToken).Forget();
            var uiPresenterGameNotification = new UIPresenterGameNotification();
            uiPresenterGameNotification.BeginAsync(gameNotificationDocumentPrefab, destroyCancellationToken).Forget();

            stateMachine.Change(StateItems);

            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    if (Keyboard.current.qKey.wasPressedThisFrame)
                    {
                        gameMessage.RequestNotification.OnNext(("Test", null, Define.NotificationType.Positive));
                    }
                })
                .RegisterTo(destroyCancellationToken);
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

        private UniTask StateProductions(CancellationToken scope)
        {
            var uiPresenterGameProductions = new UIPresenterGameProductions();
            uiPresenterGameProductions.BeginAsync(gameProductionsDocumentPrefab, scope).Forget();
            return UniTask.CompletedTask;
        }

        private UniTask StateQuests(CancellationToken scope)
        {
            var uiPresenterGameQuests = new UIPresenterGameQuests();
            uiPresenterGameQuests.BeginAsync(gameQuestsDocumentPrefab, scope).Forget();
            return UniTask.CompletedTask;
        }

        private UniTask StateEquipment(CancellationToken scope)
        {
            var uiPresenterGameEquipment = new UIPresenterGameEquipment();
            uiPresenterGameEquipment.BeginAsync(gameEquipmentDocumentPrefab, scope).Forget();
            return UniTask.CompletedTask;
        }

        private UniTask StateRiverFishing(CancellationToken scope)
        {
            var uiPresenterGameFishing = new UIPresenterGameFishing();
            uiPresenterGameFishing.BeginAsync(
                    gameRiverFishingDocumentPrefab,
                    Define.FishingType.River,
                    scope
                )
                .Forget();
            return UniTask.CompletedTask;
        }

        private UniTask StateSeaFishing(CancellationToken scope)
        {
            var uiPresenterGameFishing = new UIPresenterGameFishing();
            uiPresenterGameFishing.BeginAsync(
                    gameSeaFishingDocumentPrefab,
                    Define.FishingType.Sea,
                    scope
                )
                .Forget();
            return UniTask.CompletedTask;
        }

        private UniTask StateFarm(CancellationToken scope)
        {
            var uiPresenterGameFarm = new UIPresenterGameFarm();
            uiPresenterGameFarm.BeginAsync(gameFarmDocumentPrefab, scope).Forget();
            return UniTask.CompletedTask;
        }

        private UniTask StateMeadow(CancellationToken scope)
        {
            var uiPresenterGameMeadow = new UIPresenterGameMeadow();
            uiPresenterGameMeadow.BeginAsync(gameMeadowDocumentPrefab, scope).Forget();
            return UniTask.CompletedTask;
        }

        private UniTask StateGrassland(CancellationToken scope)
        {
            var uiPresenterGameBattle = new UIPresenterGameBattle();
            uiPresenterGameBattle.BeginAsync(gameBattleDocumentPrefab, Define.DungeonType.Grassland, scope).Forget();
            return UniTask.CompletedTask;
        }
    }
}
