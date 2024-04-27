using System.Linq;
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
    public sealed class UIPresenterGameMeal
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var gameController = TinyServiceLocator.Resolve<GameController>();
            var masterData = TinyServiceLocator.Resolve<MasterData>();
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
                    await gameController.ProcessMealAsync(itemId);
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
