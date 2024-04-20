using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using R3.Triggers;
using SCD;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UIPresenterGameFishing
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, MasterData.FishingSpec.DictionaryList fishingDesignData, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var stateMachine = new TinyStateMachine();
            var rootCastButton = document.Q("Root.CastButton");
            var rootStrikeButton = document.Q("Root.StrikeButton");
            var rootHitIcon = document.Q("Root.HitIcon");
            var currentFishingSpec = default(MasterData.FishingSpec);
            var currentContentsRecord = default(Contents.Record);
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
                        currentFishingSpec = fishingDesignData.List[Random.Range(0, fishingDesignData.List.Count)];
                        currentContentsRecord = currentFishingSpec.ToContentsRecord();
                        if (currentContentsRecord.IsCompleted(TinyServiceLocator.Resolve<GameData>().Stats))
                        {
                            stateMachine.Change(StateCast);
                        }
                        else
                        {
                            GameUtility.ShowContentsConditionsNotification(currentContentsRecord);
                        }
                    })
                    .RegisterTo(scope);
                return UniTask.CompletedTask;
            }

            UniTask StateCast(CancellationToken scope)
            {
                var hitSeconds = Random.Range(
                    currentFishingSpec.WaitSecondsMin,
                    currentFishingSpec.WaitSecondsMax
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
                var postponementSeconds = currentFishingSpec.PostponementSeconds;
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
                        TinyServiceLocator.Resolve<GameController>().CollectingAsync(currentContentsRecord).Forget();
                        stateMachine.Change(StateIdle);
                    })
                    .RegisterTo(scope);
                await UniTask.WaitUntilCanceled(scope);
                rootHitIcon.SetActive(false);
            }
        }
    }
}
