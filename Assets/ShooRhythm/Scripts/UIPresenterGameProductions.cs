using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UIPresenterGameProductions
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var draggableItemsDocument = document.Q<HKUIDocument>("DraggableItems");
            var uiPresenterDraggableItems = new UIPresenterGameSelectableItems();
            uiPresenterDraggableItems.BeginAsync(draggableItemsDocument, cancellationToken).Forget();

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }
        }
    }
}
