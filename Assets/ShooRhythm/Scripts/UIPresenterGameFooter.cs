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
            CreateListElement("道具");
            CreateListElement("採集");

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }

            void CreateListElement(string text)
            {
                var element = Object.Instantiate(listElementPrefab, listElementParent);
                element.Q<TMP_Text>("Text").text = text;
                element.Q<Button>("Button").OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        Debug.Log(text);
                    })
                    .RegisterTo(element.destroyCancellationToken);
            }
        }
    }
}
