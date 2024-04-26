using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
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
        public static async UniTask PlayAcquireItemEffectAsync(HKUIDocument document, RectTransform parent, Sprite icon, CancellationToken cancellationToken)
        {
            var effectPrefab = document.Q<HKUIDocument>("AcquireItemEffect");
            var effect = Object.Instantiate(effectPrefab, parent);
            effect.Q<Image>("Icon").sprite = icon;
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
