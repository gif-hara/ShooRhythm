using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;
using UnitySequencerSystem;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UIPresenterGameMeal
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var gameController = TinyServiceLocator.Resolve<GameController>();
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var selectItemsDocument = document.Q<HKUIDocument>("SelectItems");
            var uiPresenterSelectItems = new UIPresenterGameSelectItems();
            uiPresenterSelectItems.BeginAsync(
                selectItemsDocument,
                x => x.Where(y => masterData.MealSpecs.ContainsKey(y.Key))
                )
                .Forget();
            uiPresenterSelectItems.OnSelectedItemAsObservable()
                .SubscribeAwait(async (itemId, ct) =>
                {
                    var availableCoolTimeIndex = gameData.CurrentUserData.GetAvailableCoolTimeIndex();
                    if (availableCoolTimeIndex == -1)
                    {
                        GameUtility.ShowRequireCoolDownNotification();
                        return;
                    }

                    var mealSpec = masterData.MealSpecs.Get(itemId);
                    if (!mealSpec.HasItems())
                    {
                        mealSpec.ShowRequireItemNotification();
                        return;
                    }

                    var result = await gameController.ProcessMealAsync(itemId);
                    if (result == Define.ProcessResultType.Success)
                    {
                        gameData.CurrentUserData.SetCoolTime(availableCoolTimeIndex, mealSpec.CoolTimeSeconds);
                        var container = new Container();
                        var sequencer = new Sequencer(container, document.Q<SequencesHolder>("SuccessSequences").Sequences);
                        sequencer.PlayAsync(ct).Forget();
                    }
                })
                .RegisterTo(cancellationToken);


            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }
        }
    }
}
