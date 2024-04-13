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
    public sealed class UIPresenterGameFooter
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var listElementParent = document.Q<Transform>("ListElementParent");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            CreateListElement("道具", "Available.Tab.Items");
            CreateListElement("採集", "Available.Tab.Collections");

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }

            void CreateListElement(string text, string isActiveStatsName)
            {
                var element = Object.Instantiate(listElementPrefab, listElementParent);
                element.Q<TMP_Text>("Text").text = text;
                element.Q<Button>("Button").OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        Debug.Log(text);
                    })
                    .RegisterTo(element.destroyCancellationToken);
                var gameData = TinyServiceLocator.Resolve<GameData>();
                element.gameObject.SetActiveIfNeed(gameData.Stats.Contains(isActiveStatsName));
                gameData.Stats.OnChanged.AsObservable()
                    .Where(x => x.Name == isActiveStatsName)
                    .Subscribe(x =>
                    {
                        element.gameObject.SetActiveIfNeed(gameData.Stats.Contains(isActiveStatsName));
                    })
                    .RegisterTo(element.destroyCancellationToken);
            }
        }
    }
}
