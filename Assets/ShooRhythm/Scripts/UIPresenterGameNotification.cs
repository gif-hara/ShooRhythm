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
    public sealed class UIPresenterGameNotification
    {
        public async UniTask BeginAsync(HKUIDocument documentPrefab, CancellationToken cancellationToken)
        {
            var document = Object.Instantiate(documentPrefab);
            var listElementParent = document.Q<Transform>("ListElementParent");
            var listElementPrefab = document.Q<HKUIDocument>("ListElementPrefab");

            TinyServiceLocator.Resolve<GameMessage>().RequestNotification
                .Subscribe(x =>
                {
                    var listElement = Object.Instantiate(listElementPrefab, listElementParent);
                    listElement.Q<TextMeshProUGUI>("Text").text = x.message;
                    listElement.Q<Image>("Icon").sprite = x.sprite;
                    listElement.Q("Root.Icon").SetActive(x.sprite != null);
                    listElement.Q<SequencerMonoBehaviour>("Sequencer.InAnimation").PlayAsync().Forget();
                })
                .RegisterTo(cancellationToken);

            await UniTask.WaitUntilCanceled(cancellationToken);

            if (document != null)
            {
                Object.Destroy(document.gameObject);
            }
        }
    }
}
