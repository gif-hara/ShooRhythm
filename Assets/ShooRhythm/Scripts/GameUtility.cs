using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using SCD;
using UnityEngine;
using UnityEngine.UI;
using UnitySequencerSystem;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameUtility
    {
        public static async UniTask PlayAcquireItemEffectAsync(HKUIDocument document, RectTransform parent, UniTask<Sprite>? loadIconTask, CancellationToken cancellationToken)
        {
            var effectPrefab = document.Q<HKUIDocument>("AcquireItemEffect");
            var effect = Object.Instantiate(effectPrefab, parent);
            if(loadIconTask != null)
            {
                effect.Q<Image>("Icon").SetIconAsync(loadIconTask.Value).Forget();
            }
            var container = new Container();
            var sequences = effect.Q<SequencesHolder>("Effect").Sequences;
            var sequencer = new Sequencer(container, sequences);
            await sequencer.PlayAsync(cancellationToken);
        }

        public static void ShowRequireCoolDownNotification()
        {
            var gameMessage = TinyServiceLocator.Resolve<GameMessage>();
            gameMessage.RequestNotification.OnNext(
                (
                    "クールダウン中です",
                    null,
                    Define.NotificationType.Negative
                )
            );
        }
    }
}
