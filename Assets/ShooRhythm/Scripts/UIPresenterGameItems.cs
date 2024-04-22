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
            var elements = new List<(int id, GameObject gameObject)>();
            var gameData = TinyServiceLocator.Resolve<GameData>();
            foreach (var i in gameData.Items)
            {
                var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(i.Key);
                var element = Object.Instantiate(elementPrefab, elementParent);
                element.Q<TMP_Text>("Text.Name").text = masterDataItem.Name;
                i.Value
                    .Subscribe(itemNumber =>
                    {
                        element.Q<TMP_Text>("Text.Number").text = itemNumber.ToString();
                    })
                    .RegisterTo(cancellationToken);
                elements.Add((i.Key, element.gameObject));
            }

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }
        }
    }
}
