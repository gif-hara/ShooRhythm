using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;
using UnityEngine.UI;

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
            var selectSlotId = -1;
            var listElementParent = document.Q<Transform>("ListElementParent");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var alreadyShowSelectItems = false;
            for (var i = 0; i < gameData.Stats.Get("Productions.MachineNumber"); i++)
            {
                var element = Object.Instantiate(listElementPrefab, listElementParent);
                ObserveListElementSlotButton(element, i);
                ObserveListElementSlotButton(element, i + 1);
                ObserveListElementSlotButton(element, i + 2);
            }

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }

            void ShowSelectableItems()
            {
                if (alreadyShowSelectItems)
                {
                    return;
                }
                alreadyShowSelectItems = true;
                var selectItemsDocument = document.Q<HKUIDocument>("SelectItems");
                var uiPresenterSelectItems = new UIPresenterGameSelectItems();
                uiPresenterSelectItems.BeginAsync(selectItemsDocument, cancellationToken).Forget();
            }

            void ObserveListElementSlotButton(HKUIDocument element, int slotId)
            {
                element.Q<Button>($"Button.Slot.{slotId % 3}").OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        selectSlotId = slotId;
                        ShowSelectableItems();
                    })
                    .RegisterTo(element.destroyCancellationToken);
            }
        }
    }
}
