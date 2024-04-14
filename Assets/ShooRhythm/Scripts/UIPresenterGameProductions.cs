using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using TMPro;
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
            HKUIDocument selectSlotElement;
            var listElementParent = document.Q<Transform>("ListElementParent");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var gameController = TinyServiceLocator.Resolve<GameController>();
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
                uiPresenterSelectItems.OnSelectedItem
                    .SubscribeAwait(async (itemId, ct) =>
                    {
                        var slotId = selectSlotId % 3;
                        var machineId = selectSlotId / 3;
                        selectSlotElement.Q<TMP_Text>($"Slot.{slotId}.Text.Name").text = TinyServiceLocator.Resolve<MasterData>().Items.Get(itemId).Name;
                        await gameController.SetStatsAsync($"Productions.Machine.{machineId}.Slot.{slotId}.ItemId", itemId);
                    })
                    .RegisterTo(cancellationToken);
            }

            void ObserveListElementSlotButton(HKUIDocument element, int slotId)
            {
                element.Q<Button>($"Slot.{slotId % 3}.Button").OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        selectSlotId = slotId;
                        selectSlotElement = element;
                        ShowSelectableItems();
                    })
                    .RegisterTo(element.destroyCancellationToken);
            }
        }
    }
}
