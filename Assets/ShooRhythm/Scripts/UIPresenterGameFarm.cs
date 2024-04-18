using System;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class UIPresenterGameFarm
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = UnityEngine.Object.Instantiate(documentPrefab);
            var listElementParent = document.Q<Transform>("ListElementParent");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var gameController = TinyServiceLocator.Resolve<GameController>();
            var selectedPlantId = -1;
            var alreadyShowSelectItems = false;
            var updateProgressScopes = new List<CancellationTokenSource>();
            for (var i = 0; i < gameData.FarmDatas.Count; i++)
            {
                var plantId = i;
                var listElement = UnityEngine.Object.Instantiate(listElementPrefab, listElementParent);
                var farmData = gameData.FarmDatas[plantId];
                updateProgressScopes.Add(null);
                farmData.SeedItemId
                    .Subscribe(x =>
                    {
                        if (farmData.IsCompleted)
                        {
                            listElement.Q<TextMeshProUGUI>("Name").text =
                                TinyServiceLocator.Resolve<MasterData>().Items.Get(farmData.SeedSpec.AcquireItemId).Name;
                        }
                        else
                        {
                            TinyServiceLocator.Resolve<MasterData>().Items.TryGetValue(x, out var masterDataItem);
                            listElement.Q<TextMeshProUGUI>("Name").text = masterDataItem?.Name ?? "Empty";
                        }
                    })
                    .RegisterTo(listElement.destroyCancellationToken);
                farmData.PlantTicks
                    .Subscribe(x =>
                    {

                        updateProgressScopes[plantId]?.Cancel();
                        updateProgressScopes[plantId]?.Dispose();
                        if (x == 0)
                        {
                            listElement.Q<Slider>("Progress").value = 0;
                        }
                        else
                        {
                            if (farmData.IsCompleted)
                            {
                                listElement.Q<Slider>("Progress").value = 1;
                                return;
                            }
                            var start = farmData.PlantTicks.Value;
                            var end = new DateTime(farmData.PlantTicks.Value).AddSeconds(farmData.SeedSpec.GrowSeconds).Ticks;
                            updateProgressScopes[plantId] = CancellationTokenSource.CreateLinkedTokenSource(
                                cancellationToken,
                                listElement.destroyCancellationToken
                            );
                            Observable.EveryUpdate(cancellationToken)
                                .Subscribe(_ =>
                                {
                                    var progress = (float)(DateTime.UtcNow.Ticks - start) / (end - start);
                                    listElement.Q<Slider>("Progress").value = (float)progress;
                                    if (progress >= 1)
                                    {
                                        updateProgressScopes[plantId]?.Cancel();
                                        updateProgressScopes[plantId]?.Dispose();
                                        updateProgressScopes[plantId] = null;
                                        var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(farmData.SeedSpec.AcquireItemId);
                                        listElement.Q<TextMeshProUGUI>("Name").text = masterDataItem.Name;
                                    }
                                })
                                .RegisterTo(updateProgressScopes[plantId].Token);
                        }
                    })
                    .RegisterTo(listElement.destroyCancellationToken);
                listElement.Q<ObservablePointerClickTrigger>("Button").OnPointerClickAsObservable()
                    .SubscribeAwait(async (_, ct) =>
                    {
                        if (farmData.IsCompleted)
                        {
                            await gameController.AcquireFarmPlantAsync(plantId);
                        }
                        else
                        {
                            selectedPlantId = plantId;
                            ShowSelectItems();
                        }
                    })
                    .AddTo(listElement);
            }

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                UnityEngine.Object.Destroy(document.gameObject);
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
                uiPresenterSelectItems.BeginAsync(
                    selectItemsDocument,
                    x =>
                    {
                        return x.Where(pair => TinyServiceLocator.Resolve<MasterData>().SeedSpecs.ContainsKey(pair.Key));
                    })
                    .Forget();
                uiPresenterSelectItems.OnSelectedItemAsObservable()
                    .SubscribeAwait(async (itemId, ct) =>
                    {
                        await gameController.SetFarmPlantItemIdAsync(selectedPlantId, itemId);
                    })
                    .RegisterTo(cancellationToken);
            }
        }
    }
}
