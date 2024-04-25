using System.Collections.Generic;
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
    public sealed class UIPresenterGameCollections
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var (elementParent, parentLayout) = document.Q<RectTransform, GridLayoutGroup>("ListElementParent");
            var elementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var gameData = TinyServiceLocator.Resolve<GameData>();
            parentLayout.SetConstraintCount();
            var elements = new List<(int id, GameObject gameObject)>();
            foreach (var i in TinyServiceLocator.Resolve<MasterData>().CollectionSpecs.List)
            {
                var masterDataItem = i.GetAcquireItemMasterData();
                var element = Object.Instantiate(elementPrefab, elementParent);
                element.Q<TMP_Text>("Text").text = i.GetAcquireItem().Name;
                SetIconAsync(element.Q<Image>("Icon"), masterDataItem.GetIconAsync()).Forget();
                var button = element.Q<ObservablePointerClickTrigger>("Button");
                button.OnPointerClickAsObservable()
                    .SubscribeAwait(async (_, ct) =>
                    {
                        var availableCoolTimeIndex = gameData.CurrentUserData.GetAvailableCoolTimeIndex();
                        if (availableCoolTimeIndex == -1)
                        {
                            GameUtility.ShowRequireCoolDownNotification();
                            return;
                        }

                        await TinyServiceLocator.Resolve<GameController>().ProcessCollectionAsync(i.Id);
                        var iconTask = i.GetAcquireItemMasterData().GetIconAsync();
                        GameUtility.PlayAcquireItemEffectAsync(document, (RectTransform)button.transform, iconTask, ct).Forget();
                        gameData.CurrentUserData.SetCoolTime(availableCoolTimeIndex, i.CoolTimeSeconds);
                    })
                    .RegisterTo(element.destroyCancellationToken);
                elements.Add((i.Id, element.gameObject));
            }

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }

            static async UniTaskVoid SetIconAsync(Image image, UniTask<Sprite> iconTask)
            {
                image.sprite = await iconTask;
                image.gameObject.SetActiveIfNeed(image.sprite != null);
            }
        }
    }
}
