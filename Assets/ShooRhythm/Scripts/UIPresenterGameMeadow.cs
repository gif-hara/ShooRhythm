using System.Collections.Generic;
using System.Linq;
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
    public sealed class UIPresenterGameMeadow
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);

            SetupAnimalButton("Root.Cow", 1);
            SetupAnimalButton("Root.Pig", 2);
            SetupAnimalButton("Root.Chicken", 3);

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }

            void SetupAnimalButton(string rootName, int meadowSpecId)
            {
                var rootElement = document.Q<HKUIDocument>(rootName);
                rootElement.Q<ObservablePointerClickTrigger>("Button").OnPointerClickAsObservable()
                    .Subscribe(_ =>
                    {
                        Debug.Log($"Clicked {rootName}");
                    })
                    .RegisterTo(cancellationToken);
            }
        }
    }
}
