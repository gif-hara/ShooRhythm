using System.Collections.Generic;
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
    public sealed class UIPresenterGameQuests
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var listElementParent = document.Q<Transform>("ListElementParent");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var availableQuests = TinyServiceLocator.Resolve<MasterData>().QuestContents.GetAvailables(gameData.Stats);
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
                foreach (var quest in availableQuests)
                {
                    var element = Object.Instantiate(listElementPrefab, listElementParent);
                    elements.Add(element);
                    var elementElementParent = element.Q<Transform>("ListElementParent");
                    var elementElementPrefab = element.Q<HKUIDocument>("ListElementPrefab");
                    element.Q<Button>("Button").OnClickAsObservable()
                        .SubscribeAwait(async (_, ct) =>
                        {
                            if (quest.IsCompleted(gameData.Stats))
                            {
                                await gameController.CollectingAsync(quest);
                                CreateElements();
                            }
                            else
                            {
                                TinyServiceLocator.Resolve<GameMessage>().RequestNotification.OnNext((
                                    "アイテムが足りません",
                                    null, 
                                    Define.NotificationType.Negative
                                    ));
                            }
                        })
                        .RegisterTo(element.destroyCancellationToken);
                    foreach (var condition in quest.Conditions)
                    {
                        var elementElement = Object.Instantiate(elementElementPrefab, elementElementParent);
                        var itemId = condition.Name.Substring("Item.".Length);
                        var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(int.Parse(itemId));
                        var isSatisfied = gameData.Stats.Get(condition.Name) >= condition.Value;
                        elementElement.Q<TMP_Text>("Text.Name").text = masterDataItem.Name;
                        elementElement.Q<TMP_Text>("Text.Number").text = condition.Value.ToString();
                        elementElement.Q("Background.Enough").SetActiveIfNeed(isSatisfied);
                        elementElement.Q("Background.NotEnough").SetActiveIfNeed(!isSatisfied);
                    }
                }
            }
        }
    }
}
