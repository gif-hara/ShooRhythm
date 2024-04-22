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
                var offsetSlotId = i * Define.MachineSlotCount;
                var element = Object.Instantiate(listElementPrefab, listElementParent);
                ObserveListElementProductButton(element, i);
                for (var j = 0; j < Define.MachineSlotCount; j++)
                {
                    ObserveListElementSlotButton(element, offsetSlotId + j);
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
                        var slotId = selectSlotId % Define.MachineSlotCount;
                        var machineId = selectSlotId / Define.MachineSlotCount;
                        selectSlotElement.Q<TMP_Text>($"Slot.{slotId}.Text.Name").text = TinyServiceLocator.Resolve<MasterData>().Items.Get(itemId).Name;
                        await gameController.SetStatsAsync($"Productions.Machine.{machineId}.Slot.{slotId}.ItemId", itemId);
                    })
                    .RegisterTo(cancellationToken);
            }

            void ObserveListElementProductButton(HKUIDocument element, int machineId)
            {
                element.Q<Button>("Product.Button").OnClickAsObservable()
                    .SubscribeAwait(async (_, ct) =>
                    {
                        var itemId = gameData.Stats.Get($"Productions.Machine.{machineId}.Product");
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
                    })
                    .RegisterTo(element.destroyCancellationToken);
                TinyServiceLocator.Resolve<GameData>().Stats.OnChangedAsObservable(cancellationToken)
                    .Subscribe(x =>
                    {
                        var startString = $"Productions.Machine.{machineId}.Product";
                        if (x.Name.StartsWith(startString, System.StringComparison.Ordinal))
                        {
                            var text = element.Q<TMP_Text>("Product.Text.Name");
                            var itemId = x.Value;
                            if (itemId == 0)
                            {
                                text.text = "Empty";
                            }
                            else
                            {
                                var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(itemId);
                                text.text = masterDataItem.Name;
                            }
                        }
                    })
                    .RegisterTo(element.destroyCancellationToken);
            }

            void ObserveListElementSlotButton(HKUIDocument element, int slotId)
            {
                element.Q<Button>($"Slot.{slotId % Define.MachineSlotCount}.Button").OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        selectSlotId = slotId;
                        selectSlotElement = element;
                        ShowSelectItems();
                    })
                    .RegisterTo(element.destroyCancellationToken);
            }
        }
    }
}
