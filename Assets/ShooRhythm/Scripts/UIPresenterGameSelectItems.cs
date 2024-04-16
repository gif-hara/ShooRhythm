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
    public sealed class UIPresenterGameSelectItems
    {
        private readonly Subject<int> onSelectedItem = new();
        public Observable<int> OnSelectedItem => onSelectedItem;

        public UniTask BeginAsync(HKUIDocument document, CancellationToken cancellationToken)
        {
            var listElementParentName = "ListElementParent";
            var elementParent = document.Q<RectTransform>(listElementParentName);
            var parentLayout = document.Q<GridLayoutGroup>(listElementParentName);
            var elementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            parentLayout.SetConstraintCount();
            var elements = new List<(int id, GameObject gameObject)>();
            foreach (var i in TinyServiceLocator.Resolve<GameData>().Items)
            {
                var element = Object.Instantiate(elementPrefab, elementParent);
                var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(i.Key);
                element.Q<TMP_Text>("Text.Name").text = masterDataItem.Name;
                element.Q<TMP_Text>("Text.Number").text = i.Value.ToString();
                element.Q<Button>("Button").OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        onSelectedItem.OnNext(i.Key);
                    })
                    .RegisterTo(element.destroyCancellationToken);
                elements.Add((i.Key, element.gameObject));
            }

            return UniTask.CompletedTask;
        }
    }
}
