using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnitySequencerSystem;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class PlaySequencer : ISequence
    {
        [SerializeField]
        private ScriptableSequences target;
        
        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            return new Sequencer(container, target.Sequences).PlayAsync(cancellationToken);
        }
    }
}
