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
    public sealed class UIPresenterGameItems
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var elementParent = document.Q<Transform>("ListElementParent");
            var parentLayout = document.Q<GridLayoutGroup>("ListElementParent");
            parentLayout.SetConstraintCount();
            var elementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var gameData = TinyServiceLocator.Resolve<GameData>();
            var elements = new List<GameObject>();
            CreateElements();
            TinyServiceLocator.Resolve<GameMessage>().AddedItem
                .Subscribe(_ =>
                {
                    CreateElements();
                })
                .RegisterTo(cancellationToken);

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }

            void CreateElements()
            {
                foreach (var element in elements)
                {
                    Object.Destroy(element);
                }
                elements.Clear();
                foreach (var i in gameData.Items)
                {
                    var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(i.Key);
                    var element = Object.Instantiate(elementPrefab, elementParent);
                    var scope = CancellationTokenSource.CreateLinkedTokenSource(
                        element.destroyCancellationToken,
                        cancellationToken
                        );
                    element.Q<TMP_Text>("Text.Name").text = masterDataItem.Name;
                    SetIconAsync(element.Q<Image>("Icon"), masterDataItem.GetIconAsync()).Forget();
                    i.Value
                        .Subscribe(itemNumber =>
                        {
                            element.Q<TMP_Text>("Text.Number").text = itemNumber.ToString();
                        })
                        .RegisterTo(scope.Token);
                    elements.Add(element.gameObject);
                }
            }

            async UniTaskVoid SetIconAsync(Image image, UniTask<Sprite> sprite)
            {
                image.sprite = await sprite;
                image.gameObject.SetActiveIfNeed(image.sprite != null);
            }
        }
    }
}
