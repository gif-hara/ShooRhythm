using System.Collections.Generic;
using System.Linq;
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
            var specs = TinyServiceLocator.Resolve<MasterData>().CollectionSpecs.List
                .Where(x => x.CollectionType == Define.CollectionType.Collection);
            foreach (var i in specs)
            {
                var element = Object.Instantiate(elementPrefab, elementParent);
                var collection = TinyServiceLocator.Resolve<MasterData>().Collections.Get(i.AcquireItemId.ToString());
                element.Q<TMP_Text>("Text").text = i.GetItem().Name;
                element.Q<PointerClickHandler>("Button").OnHandledAsObservable()
                    .SubscribeAwait(async (_, ct) =>
                    {
                        await TinyServiceLocator.Resolve<GameController>()
                            .CollectingAsync(collection);
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
