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
    public sealed class UIPresenterGameFooter
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var buttonListElementParent = document.Q<Transform>("ButtonListElementParent");
            var buttonListElementPrefab = document.Q<HKUIDocument>("ButtonListElementPrefab");
            var coolTimeListElementParent = document.Q<Transform>("CoolTimeListElementParent");
            var coolTimeListElementPrefab = document.Q<HKUIDocument>("CoolTimeListElementPrefab");
            foreach (var i in TinyServiceLocator.Resolve<GameDesignData>().Footers)
            {
                CreateButtonElement(i.FooterName, i.TabType);
            }
            for (var i = 0; i < gameData.CurrentUserData.coolTimeData.Count; i++)
            {
                CreateCoolTimeElement(i);
            }

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }

            void CreateButtonElement(string text, Define.TabType tabType)
            {
                var element = Object.Instantiate(buttonListElementPrefab, buttonListElementParent);
                element.Q<TMP_Text>("Text").text = text;
                element.Q<ObservablePointerClickTrigger>("Button").OnPointerClickAsObservable()
                    .Subscribe(_ =>
                    {
                        TinyServiceLocator.Resolve<GameMessage>().RequestChangeTab.OnNext(tabType);
                    })
                    .RegisterTo(element.destroyCancellationToken);
                element.gameObject.SetActiveIfNeed(gameData.ContentAvailabilities.Contains(tabType.ToContentAvailabilityName()));
                TinyServiceLocator.Resolve<GameMessage>().AddedContentAvailability
                    .Subscribe(x =>
                    {
                        element.gameObject.SetActiveIfNeed(gameData.ContentAvailabilities.Contains(tabType.ToContentAvailabilityName()));
                    })
                    .RegisterTo(element.destroyCancellationToken);
            }

            void CreateCoolTimeElement(int index)
            {
                var element = Object.Instantiate(coolTimeListElementPrefab, coolTimeListElementParent);
                var slider = element.Q<Slider>("Slider");
                var maxObject = element.Q("MaxObject");
                var coolTimeData = gameData.CurrentUserData.coolTimeData[index];
                coolTimeData.CoolTime
                    .Subscribe(x =>
                    {
                        var max = coolTimeData.Max;
                        slider.value = max == 0 ? 1 : (max - x) / max;
                        maxObject.SetActiveIfNeed(max == 0 || x <= 0);
                    })
                    .RegisterTo(element.destroyCancellationToken);
            }
        }
    }
}
