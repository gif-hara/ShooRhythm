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
            var selectMachineId = -1;
            var selectSlotId = -1;
            const string listElementParentName = "ListElementParent";
            var listElementParent = document.Q<Transform>(listElementParentName);
            var parentLayout = document.Q<GridLayoutGroup>(listElementParentName);
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            parentLayout.SetConstraintCount();
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var gameController = TinyServiceLocator.Resolve<GameController>();
            var alreadyShowSelectItems = false;
            for (var i = 0; i < gameData.ProductMachineData.Count; i++)
            {
                var element = Object.Instantiate(listElementPrefab, listElementParent);
                var productMachineData = gameData.ProductMachineData[i];
                var button = element.Q<Button>("Product.Button");
                button.OnClickAsObservable()
                    .SubscribeAwait(async (_, ct) =>
                    {
                        var itemId = productMachineData.productItemId.Value;
                        if (itemId == 0)
                        {
                            return;
                        }
                        var productionSpec = TinyServiceLocator.Resolve<MasterData>().ProductionSpecs.Get(itemId);
                        if (productionSpec == null)
                        {
                            Debug.LogWarning($"Collection is null. CollectionId:{itemId}");
                            return;
                        }
                        var contentsRecord = productionSpec.ToContentsRecord();
                        if (!contentsRecord.IsCompleted(gameData.Stats))
                        {
                            Debug.Log("TODO Not Completed Collection");
                            return;
                        }
                        await gameController.ApplyRewardAsync(contentsRecord);
                        GameUtility.PlayAcquireItemEffectAsync(document, (RectTransform)button.transform, ct).Forget();
                    })
                    .RegisterTo(element.destroyCancellationToken);
                productMachineData.productItemId
                    .Subscribe(x =>
                    {
                        var text = element.Q<TMP_Text>("Product.Text.Name");
                        if (x == 0)
                        {
                            text.text = "Empty";
                        }
                        else
                        {
                            var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(x);
                            text.text = masterDataItem.Name;
                        }
                    })
                    .RegisterTo(element.destroyCancellationToken);
                for (var j = 0; j < Define.MachineSlotCount; j++)
                {
                    var machineId = i;
                    var slotId = j;
                    element.Q<Button>($"Slot.{slotId % Define.MachineSlotCount}.Button").OnClickAsObservable()
                        .Subscribe(_ =>
                        {
                            selectMachineId = machineId;
                            selectSlotId = slotId;
                            ShowSelectItems();
                        })
                        .RegisterTo(element.destroyCancellationToken);
                    productMachineData.slotItemIds[j]
                        .Subscribe(x =>
                        {
                            TinyServiceLocator.Resolve<MasterData>().Items.TryGetValue(x, out var masterDataItem);
                            element.Q<TMP_Text>($"Slot.{slotId}.Text.Name").text = masterDataItem?.Name ?? "Empty";
                        })
                        .RegisterTo(cancellationToken);
                }
            }

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }

            void ShowSelectItems()
            {
                if (alreadyShowSelectItems)
                {
                    return;
                }
                alreadyShowSelectItems = true;
                var selectItemsDocument = document.Q<HKUIDocument>("SelectItems");
                var uiPresenterSelectItems = new UIPresenterGameSelectItems();
                uiPresenterSelectItems.BeginAsync(selectItemsDocument, x => x).Forget();
                uiPresenterSelectItems.OnSelectedItemAsObservable()
                    .SubscribeAwait(async (itemId, ct) =>
                    {
                        await gameController.SetProductMachineSlotAsync(selectMachineId, selectSlotId, itemId);
                    })
                    .RegisterTo(cancellationToken);
            }
        }
    }
}
