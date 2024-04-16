using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using R3.Triggers;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UIPresenterGameFishing
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var stateMachine = new TinyStateMachine();
            var gameDesignData = TinyServiceLocator.Resolve<GameDesignData>();
            var rootCastButton = document.Q("Root.CastButton");
            var rootStrikeButton = document.Q("Root.StrikeButton");
            var rootHitIcon = document.Q("Root.HitIcon");
            stateMachine.Change(StateIdle);
           
            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null && document.gameObject != null)
            {
                Object.Destroy(document.gameObject);
            }

            UniTask StateIdle(CancellationToken scope)
            {
                rootCastButton.SetActive(true);
                rootStrikeButton.SetActive(false);
                
                document.Q<ObservablePointerClickTrigger>("Button.Cast").OnPointerClickAsObservable()
                    .Subscribe(_ =>
                    {
                        stateMachine.Change(StateCast);
                    })
                    .RegisterTo(scope);
                return UniTask.CompletedTask;
            }
            
            UniTask StateCast(CancellationToken scope)
            {
                var hitSeconds = Random.Range(
                    gameDesignData.RiverFishingDesignData.WaitSecondsMin,
                    gameDesignData.RiverFishingDesignData.WaitSecondsMax
                    );
                var currentSeconds = 0f;
                rootCastButton.SetActive(false);
                rootStrikeButton.SetActive(true);
                Observable.EveryUpdate(UnityFrameProvider.Update, scope)
                    .Subscribe(_ =>
                    {
                        currentSeconds += Time.deltaTime;
                        if(currentSeconds >= hitSeconds)
                        {
                            stateMachine.Change(StateHit);
                        }
                    })
                    .RegisterTo(scope);
                document.Q<ObservablePointerClickTrigger>("Button.Strike").OnPointerClickAsObservable()
                    .Subscribe(_ =>
                    {
                        stateMachine.Change(StateIdle);
                    })
                    .RegisterTo(scope);
                return UniTask.CompletedTask;
            }
            
            async UniTask StateHit(CancellationToken scope)
            {
                var postponementSeconds = gameDesignData.RiverFishingDesignData.PostponementSeconds;
                var currentSeconds = 0f;
                rootHitIcon.SetActive(true);
                Observable.EveryUpdate(UnityFrameProvider.Update, scope)
                    .Subscribe(_ =>
                    {
                        currentSeconds += Time.deltaTime;
                        if(currentSeconds >= postponementSeconds)
                        {
                            stateMachine.Change(StateIdle);
                        }
                    })
                    .RegisterTo(scope);
                document.Q<ObservablePointerClickTrigger>("Button.Strike").OnPointerClickAsObservable()
                    .Subscribe(_ =>
                    {
                        var collectionRecord = TinyServiceLocator.Resolve<MasterData>().Collections.Get(gameDesignData.RiverFishingDesignData.AcquireCollectionSpecName);
                        TinyServiceLocator.Resolve<GameController>().CollectingAsync(collectionRecord).Forget();
                        stateMachine.Change(StateIdle);
                    })
                    .RegisterTo(scope);
                await UniTask.WaitUntilCanceled(scope);
                rootHitIcon.SetActive(false);
            }
        }
    }
}
