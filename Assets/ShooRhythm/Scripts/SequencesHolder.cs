using System.Collections.Generic;
using UnityEngine;
using UnitySequencerSystem;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SequencesHolder : MonoBehaviour
    {
        [SerializeReference, SubclassSelector]
        private List<ISequence> sequences = default;

        public List<ISequence> Sequences => sequences;
    }
}
