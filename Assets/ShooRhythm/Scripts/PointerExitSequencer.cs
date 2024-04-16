using System.Collections.Generic;
using R3;
using R3.Triggers;
using UnityEngine;
using UnitySequencerSystem;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PointerExitSequencer : MonoBehaviour
    {
        [SerializeField]
        private ObservablePointerExitTrigger trigger;
        
        [SerializeReference, SubclassSelector]
        private List<ISequence> sequences;

        private void Awake()
        {
            trigger.OnPointerExitAsObservable()
                .SubscribeAwait(async (_, ct) =>
                {
                    var container = new Container();
                    var sequencer = new Sequencer(container, sequences);
                    await sequencer.PlayAsync(ct);
                })
                .AddTo(this);
        }
    }
}
