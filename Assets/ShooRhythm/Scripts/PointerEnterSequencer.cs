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
    public sealed class PointerEnterSequencer : MonoBehaviour
    {
        [SerializeField]
        private ObservablePointerEnterTrigger trigger;
        
        [SerializeReference, SubclassSelector]
        private List<ISequence> sequences;

        private void Awake()
        {
            trigger.OnPointerEnterAsObservable()
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
