using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UIPresenterGameSelectItems
    {
        private readonly Subject<int> onSelectedItem = new();
        public Observable<int> OnSelectedItemAsObservable() => onSelectedItem;

        public UniTask BeginAsync(HKUIDocument document, Func<Dictionary<int, ReactiveProperty<int>>, IEnumerable<KeyValuePair<int, ReactiveProperty<int>>>> itemSelector)
        {
            var (elementParent, parentLayout) = document.Q<RectTransform, GridLayoutGroup>("ListElementParent");
            var elementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            parentLayout.SetConstraintCount();
            var elements = new List<GameObject>();
            CreateElements();
            
            TinyServiceLocator.Resolve<GameMessage>().AddedItem
                .Subscribe(_ =>
                {
                    CreateElements();
                })
                .RegisterTo(document.destroyCancellationToken);

            return UniTask.CompletedTask;

            void CreateElements()
            {
                foreach (var i in elements)
                {
                    Object.Destroy(i);
                }
                elements.Clear();
                foreach (var i in itemSelector(TinyServiceLocator.Resolve<GameData>().Items))
                {
                    var element = UnityEngine.Object.Instantiate(elementPrefab, elementParent);
                    var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(i.Key);
                    element.Q<TMP_Text>("Text.Name").text = masterDataItem.Name;
                    i.Value
                        .Subscribe(itemNumber =>
                        {
                            element.Q<TMP_Text>("Text.Number").text = itemNumber.ToString();
                        })
                        .RegisterTo(element.destroyCancellationToken);
                    element.Q<Button>("Button").OnClickAsObservable()
                        .Subscribe(_ =>
                        {
                            onSelectedItem.OnNext(i.Key);
                        })
                        .RegisterTo(element.destroyCancellationToken);
                    elements.Add(element.gameObject);
                }
            }
        }
    }
}
