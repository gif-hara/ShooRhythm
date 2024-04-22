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
            var listElementParentName = "ListElementParent";
            var elementParent = document.Q<RectTransform>(listElementParentName);
            var parentLayout = document.Q<GridLayoutGroup>(listElementParentName);
            var elementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            parentLayout.SetConstraintCount();
            var elements = new List<(int id, GameObject gameObject)>();
            foreach (var i in TinyServiceLocator.Resolve<MasterData>().CollectionSpecs.List)
            {
                var element = Object.Instantiate(elementPrefab, elementParent);
                var contentsRecord = i.ToContentsRecord();
                element.Q<TMP_Text>("Text").text = i.GetAcquireItem().Name;
                var button = element.Q<ObservablePointerClickTrigger>("Button");
                button.OnPointerClickAsObservable()
                    .SubscribeAwait(async (_, ct) =>
                    {
                        await TinyServiceLocator.Resolve<GameController>()
                            .ApplyRewardAsync(contentsRecord);
                        GameUtility.PlayAcquireItemEffectAsync(document, (RectTransform)button.transform, ct).Forget();
                    })
                    .RegisterTo(element.destroyCancellationToken);
                elements.Add((i.Id, element.gameObject));
            }

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }
        }
    }
}
