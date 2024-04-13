using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using TMPro;
using UnityEngine;

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
            var elementPrefab = document.Q<HKUIDocument>("ListElementPrefab");
            var elements = new List<(int id, GameObject gameObject)>();
            var gameData = TinyServiceLocator.Resolve<GameData>();
            foreach (var i in gameData.Items)
            {
                var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(i.Key);
                var element = Object.Instantiate(elementPrefab, elementParent);
                element.Q<TMP_Text>("Text.Name").text = masterDataItem.Name;
                element.Q<TMP_Text>("Text.Number").text = i.Value.ToString();
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
