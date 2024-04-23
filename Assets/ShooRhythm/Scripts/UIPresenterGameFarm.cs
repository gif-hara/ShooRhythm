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
                CreateElement(i);
            }

            TinyServiceLocator.Resolve<GameMessage>().AddedFarmData
                .Subscribe(_ =>
                {
                    CreateElement(gameData.FarmDatas.Count - 1);
                })
                .RegisterTo(cancellationToken);

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                UnityEngine.Object.Destroy(document.gameObject);
            }

            void CreateElement(int index)
            {
                var listElement = UnityEngine.Object.Instantiate(listElementPrefab, listElementParent);
                var farmData = gameData.FarmDatas[index];
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

                        updateProgressScopes[index]?.Cancel();
                        updateProgressScopes[index]?.Dispose();
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
                            updateProgressScopes[index] = CancellationTokenSource.CreateLinkedTokenSource(
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
                                        updateProgressScopes[index]?.Cancel();
                                        updateProgressScopes[index]?.Dispose();
                                        updateProgressScopes[index] = null;
                                        var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(farmData.SeedSpec.AcquireItemId);
                                        listElement.Q<TextMeshProUGUI>("Name").text = masterDataItem.Name;
                                    }
                                })
                                .RegisterTo(updateProgressScopes[index].Token);
                        }
                    })
                    .RegisterTo(listElement.destroyCancellationToken);
                var button = listElement.Q<ObservablePointerClickTrigger>("Button");
                button.OnPointerClickAsObservable()
                    .SubscribeAwait(async (_, ct) =>
                    {
                        if (farmData.IsCompleted)
                        {
                            await gameController.AcquireFarmPlantAsync(index);
                            GameUtility.PlayAcquireItemEffectAsync(document, (RectTransform)button.transform, null, ct).Forget();
                        }
                        else
                        {
                            selectedPlantId = index;
                            ShowSelectItems();
                        }
                    })
                    .AddTo(listElement);
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
