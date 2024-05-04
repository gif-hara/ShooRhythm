using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using R3.Triggers;
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
            var (listElementParent, parentLayout) = document.Q<Transform, GridLayoutGroup>("ListElementParent");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            parentLayout.SetConstraintCount();
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var gameController = TinyServiceLocator.Resolve<GameController>();
            var alreadyShowSelectItems = false;
            for (var i = 0; i < gameData.ProductMachineData.Count; i++)
            {
                ObserveProductMachine(i);
            }

            TinyServiceLocator.Resolve<GameMessage>().AddedProductMachineData
                .Subscribe(_ =>
                {
                    ObserveProductMachine(gameData.ProductMachineData.Count - 1);
                })
                .RegisterTo(cancellationToken);

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }

            void ObserveProductMachine(int machineIndex)
            {
                var productMachineData = gameData.ProductMachineData[machineIndex];
                var element = Object.Instantiate(listElementPrefab, listElementParent);
                var button = element.Q<ObservablePointerClickTrigger>("Product.Button");
                button.OnPointerClickAsObservable()
                    .SubscribeAwait(async (_, ct) =>
                    {
                        var availableCoolTimeIndex = gameData.CurrentUserData.GetAvailableCoolTimeIndex();
                        if (availableCoolTimeIndex == -1)
                        {
                            GameUtility.ShowRequireCoolDownNotification();
                            return;
                        }
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
                        var conditions = productionSpec.GetProductionNeedItems();
                        if (!conditions.HasItems())
                        {
                            Debug.Log("TODO Not Completed Collection");
                            return;
                        }
                        await gameController.ProcessProductionAcquireProductAsync(itemId);
                        GameUtility.PlayAcquireItemEffectAsync(document, (RectTransform)button.transform, null, ct).Forget();
                        gameData.CurrentUserData.SetCoolTime(availableCoolTimeIndex, productionSpec.CoolTimeSeconds);
                    })
                    .RegisterTo(element.destroyCancellationToken);
                productMachineData.productItemId
                    .Subscribe(x =>
                    {
                        var text = element.Q<TMP_Text>("Product.Text.Name");
                        var icon = element.Q<Image>("Product.Icon");
                        if (x == 0)
                        {
                            text.text = "Empty";
                            icon.sprite = null;
                            icon.enabled = false;
                        }
                        else
                        {
                            var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(x);
                            text.text = masterDataItem.Name;
                            text.enabled = masterDataItem.Icon == null;
                            icon.sprite = masterDataItem.Icon;
                            icon.enabled = masterDataItem.Icon != null;
                        }
                    })
                    .RegisterTo(element.destroyCancellationToken);
                for (var j = 0; j < Define.MachineSlotCount; j++)
                {
                    var slotId = j;
                    element.Q<ObservablePointerClickTrigger>($"Slot.{slotId}.Button").OnPointerClickAsObservable()
                        .Subscribe(_ =>
                        {
                            selectMachineId = machineIndex;
                            if (selectSlotId != -1)
                            {
                                element.Q($"Slot.{selectSlotId}.SelectedObject").SetActiveIfNeed(false);
                            }
                            selectSlotId = slotId;
                            element.Q($"Slot.{slotId}.SelectedObject").SetActiveIfNeed(true);
                            ShowSelectItems();
                        })
                        .RegisterTo(element.destroyCancellationToken);
                    productMachineData.slotItemIds[j]
                        .Subscribe(x =>
                        {
                            TinyServiceLocator.Resolve<MasterData>().Items.TryGetValue(x, out var masterDataItem);
                            var nameText = element.Q<TMP_Text>($"Slot.{slotId}.Text.Name");
                            var icon = element.Q<Image>($"Slot.{slotId}.Icon");
                            if (masterDataItem == null)
                            {
                                nameText.text = "Empty";
                                icon.sprite = null;
                                icon.enabled = false;
                            }
                            else
                            {
                                nameText.text = masterDataItem.Name;
                                nameText.enabled = masterDataItem.Icon == null;
                                icon.sprite = masterDataItem.Icon;
                                icon.enabled = masterDataItem.Icon != null;
                            }
                        })
                        .RegisterTo(cancellationToken);
                }
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
                        await gameController.ProcessProductionSetSlotAsync(selectMachineId, selectSlotId, itemId);
                    })
                    .RegisterTo(cancellationToken);
            }
        }
    }
}
