using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UIPresenterGameFarm
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var listElementParent = document.Q<Transform>("ListElementParent");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var selectedPlantId = -1;
            var alreadyShowSelectItems = false;
            for (var i = 0; i < gameData.Stats.Get("Farm.PlantNumber"); i++)
            {
                var plantId = i;
                var listElement = Object.Instantiate(listElementPrefab, listElementParent);
                gameData.Stats.OnChangedAsObservable(cancellationToken)
                    .Subscribe(x =>
                    {
                        if (x.Name == $"Farm.Plant.{plantId}.ItemId")
                        {
                            var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(x.Value);
                            listElement.Q<TextMeshProUGUI>("Name").text = masterDataItem.Name;
                        }
                    })
                    .RegisterTo(cancellationToken);
                listElement.Q<ObservablePointerClickTrigger>("Button").OnPointerClickAsObservable()
                    .Subscribe(_ =>
                    {
                        selectedPlantId = plantId;
                        ShowSelectItems();
                    })
                    .AddTo(listElement);
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
                        await TinyServiceLocator.Resolve<GameController>().SetStatsAsync($"Farm.Plant.{selectedPlantId}.ItemId", itemId);
                    })
                    .RegisterTo(cancellationToken);
            }
        }
    }
}
