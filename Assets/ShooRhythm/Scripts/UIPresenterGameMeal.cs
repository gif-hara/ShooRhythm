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
            var selectItemsDocument = document.Q<HKUIDocument>("SelectItems");
            var uiPresenterSelectItems = new UIPresenterGameSelectItems();
            uiPresenterSelectItems.BeginAsync(selectItemsDocument, x => x).Forget();
            uiPresenterSelectItems.OnSelectedItemAsObservable()
                .SubscribeAwait(async (itemId, ct) =>
                {
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
