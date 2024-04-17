using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnitySequencerSystem;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SequencerMonoBehaviour : MonoBehaviour
    {
        [SerializeReference, SubclassSelector]
        private List<ISequence> sequences = new();

        public UniTask PlayAsync()
        {
            var container = new Container();
            var sequencer = new Sequencer(container, sequences);
            return sequencer.PlayAsync(destroyCancellationToken);
        }
    }
}
