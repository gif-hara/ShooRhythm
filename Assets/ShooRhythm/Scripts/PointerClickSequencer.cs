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
    public sealed class PointerClickSequencer : MonoBehaviour
    {
        [SerializeField]
        private ObservablePointerClickTrigger trigger;
        
        [SerializeReference, SubclassSelector]
        private List<ISequence> sequences;

        private void Awake()
        {
            trigger.OnPointerClickAsObservable()
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
