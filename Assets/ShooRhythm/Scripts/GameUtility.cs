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
        public static void ShowContentsConditionsNotification(Contents.Record contentsRecord)
        {
            var gameMessage = TinyServiceLocator.Resolve<GameMessage>();
            foreach (var i in contentsRecord.Conditions)
            {
                if (i.Name.StartsWith("Item.", System.StringComparison.Ordinal))
                {
                    var itemId = int.Parse(i.Name.Substring(5));
                    var masterDataItem = TinyServiceLocator.Resolve<MasterData>().Items.Get(itemId);
                    gameMessage.RequestNotification.OnNext(
                        (
                            $"{masterDataItem.Name}が{i.Value}個必要です",
                            null,
                            Define.NotificationType.Negative
                        )
                    );
                }
                else
                {
                    Debug.LogWarning($"Unknown record name: {i.Name}");
                }
            }
        }
        
        public static async UniTask PlayAcquireItemEffectAsync(HKUIDocument document, RectTransform parent, UniTask<Sprite>? iconTask, CancellationToken cancellationToken)
        {
            var effectPrefab = document.Q<HKUIDocument>("AcquireItemEffect");
            var effect = Object.Instantiate(effectPrefab, parent);
            if (iconTask != null)
            {
                effect.Q<Image>("Icon").sprite = await iconTask.Value;
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
