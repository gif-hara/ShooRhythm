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
        public async UniTask BeginAsync(HKUIDocument documentPrefab, GameDesignData.FishingDesignData fishingDesignData, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var stateMachine = new TinyStateMachine();
            var rootCastButton = document.Q("Root.CastButton");
            var rootStrikeButton = document.Q("Root.StrikeButton");
            var rootHitIcon = document.Q("Root.HitIcon");
            var collectionRecord = TinyServiceLocator.Resolve<MasterData>().Collections.Get(fishingDesignData.AcquireCollectionSpecName);
            rootHitIcon.SetActive(false);
            stateMachine.Change(StateIdle);

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null && document.gameObject != null)
            {
                Object.Destroy(document.gameObject);
            }

            UniTask StateIdle(CancellationToken scope)
            {
                rootHitIcon.SetActive(false);
                rootCastButton.SetActive(true);
                rootStrikeButton.SetActive(false);

                document.Q<ObservablePointerClickTrigger>("Button.Cast").OnPointerClickAsObservable()
                    .Subscribe(_ =>
                    {
                        if (collectionRecord.IsCompleted(TinyServiceLocator.Resolve<GameData>().Stats))
                        {
                            stateMachine.Change(StateCast);
                        }
                        else
                        {
                            TinyServiceLocator.Resolve<GameMessage>().RequestNotification.OnNext(("釣り竿が必要です", null, Define.NotificationType.Negative));
                        }
                    })
                    .RegisterTo(scope);
                return UniTask.CompletedTask;
            }

            UniTask StateCast(CancellationToken scope)
            {
                var hitSeconds = Random.Range(
                    fishingDesignData.WaitSecondsMin,
                    fishingDesignData.WaitSecondsMax
                    );
                var currentSeconds = 0f;
                rootCastButton.SetActive(false);
                rootStrikeButton.SetActive(true);
                Observable.EveryUpdate(UnityFrameProvider.Update, scope)
                    .Subscribe(_ =>
                    {
                        currentSeconds += Time.deltaTime;
                        if (currentSeconds >= hitSeconds)
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
                var postponementSeconds = fishingDesignData.PostponementSeconds;
                var currentSeconds = 0f;
                rootHitIcon.SetActive(true);
                Observable.EveryUpdate(UnityFrameProvider.Update, scope)
                    .Subscribe(_ =>
                    {
                        currentSeconds += Time.deltaTime;
                        if (currentSeconds >= postponementSeconds)
                        {
                            stateMachine.Change(StateIdle);
                        }
                    })
                    .RegisterTo(scope);
                document.Q<ObservablePointerClickTrigger>("Button.Strike").OnPointerClickAsObservable()
                    .Subscribe(_ =>
                    {
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
