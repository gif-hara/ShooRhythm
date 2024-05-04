using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UIPresenterGameEnhance
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var elementParent = document.Q<Transform>("ListElementParent");
            var elementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var userData = gameData.CurrentUserData;
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            var gameDegisnData = TinyServiceLocator.Resolve<GameDesignData>();
            var gameMessage = TinyServiceLocator.Resolve<GameMessage>();
            var gameController = TinyServiceLocator.Resolve<GameController>();
            var elements = new List<HKUIDocument>();
            CreateElements();

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }

            void CreateElements()
            {
                foreach (var element in elements)
                {
                    Object.Destroy(element.gameObject);
                }
                elements.Clear();
                foreach (var i in Enum.GetValues(typeof(Define.EnhanceType)))
                {
                    var type = (Define.EnhanceType)i;
                    var currentLevel = userData.GetEnhanceLevel(type);
                    var groupId = type.GetEnhanceGroupId(currentLevel + 1);
                    if (masterData.EnhanceSpecs.TryGetValue(groupId, out var enhanceSpecs))
                    {
                        var element = Object.Instantiate(elementPrefab, elementParent);
                        element.Q<TMP_Text>("Text.Title").SetText(gameDegisnData.EnhanceNames.Find(x => x.EnhanceType == type).Name);
                        element.Q<TMP_Text>("Text.Level").SetText("Lv.{0}", currentLevel);
                        foreach (var j in enhanceSpecs)
                        {
                            var elementElement = Object.Instantiate(
                                element.Q<HKUIDocument>("ListElementPrefab"),
                                element.Q<Transform>("ListElementParent")
                                );
                            var masterDataItem = j.GetMasterDataItem();
                            gameData.GetItemNumberReactiveProperty(j.NeedItemId)
                                .Subscribe(x =>
                                {
                                    elementElement.Q<TMP_Text>("Text.Number").SetText("{0}", gameData.GetItem(j.NeedItemId));
                                    elementElement.Q<TMP_Text>("Text.NeedValue").SetText("{0}", j.NeedItemAmount);
                                    var hasItems = j.HasItems();
                                    elementElement.Q("ItemNumber.Background.Enough").SetActiveIfNeed(hasItems);
                                    elementElement.Q("ItemNumber.Background.NotEnough").SetActiveIfNeed(!hasItems);
                                })
                                .RegisterTo(elementElement.destroyCancellationToken);
                            elementElement.Q<TMP_Text>("Text.Name").SetText(masterDataItem.Name);
                            elementElement.Q<Image>("Icon").sprite = masterDataItem.Icon;
                        }
                        element.Q<ObservablePointerClickTrigger>("Button").OnPointerClickAsObservable()
                            .SubscribeAwait(async (_, ct) =>
                            {
                                if (enhanceSpecs.HasItems())
                                {
                                    await gameController.ProcessEnhanceAsync(type);
                                }
                                else
                                {
                                    enhanceSpecs.ShowRequireItemNotification();
                                }
                            })
                            .RegisterTo(element.destroyCancellationToken);
                    }
                }
            }
        }
    }
}
