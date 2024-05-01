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
    public sealed class UIPresenterGameQuests
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var listElementParent = document.Q<Transform>("ListElementParent");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var availableQuestIds = TinyServiceLocator.Resolve<MasterData>().QuestSpecs.List
                .Where(x => !x.GetQuestIgnores().IsAvailableAny())
                .Where(x => x.GetQuestRequires().IsAvailableAll())
                .Select(x => x.Id);
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
                foreach (var id in availableQuestIds)
                {
                    var questSpec = TinyServiceLocator.Resolve<MasterData>().QuestSpecs.Get(id);
                    var conditions = questSpec.GetQuestConditions();
                    var element = Object.Instantiate(listElementPrefab, listElementParent);
                    elements.Add(element);
                    var elementElementParent = element.Q<Transform>("ListElementParent");
                    var elementElementPrefab = element.Q<HKUIDocument>("ListElementPrefab");
                    element.Q<ObservablePointerClickTrigger>("Button").OnPointerClickAsObservable()
                        .SubscribeAwait(async (_, ct) =>
                        {
                            if (conditions.HasItems())
                            {
                                await gameController.ProcessQuestAsync(id);
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
                    foreach (var condition in conditions)
                    {
                        var elementElement = Object.Instantiate(elementElementPrefab, elementElementParent);
                        var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(condition.NeedItemId);
                        var isSatisfied = condition.HasItems();
                        var icon = elementElement.Q<Image>("Icon");
                        var nameText = elementElement.Q<TMP_Text>("Text.Name");
                        icon.sprite = masterDataItem.Icon;
                        icon.enabled = masterDataItem.Icon != null;
                        nameText.text = masterDataItem.Name;
                        nameText.enabled = masterDataItem.Icon == null;
                        elementElement.Q<TMP_Text>("Text.Number").text = condition.NeedItemAmount.ToString();
                        elementElement.Q("Background.Enough").SetActiveIfNeed(isSatisfied);
                        elementElement.Q("Background.NotEnough").SetActiveIfNeed(!isSatisfied);
                    }
                }
            }
        }
    }
}
