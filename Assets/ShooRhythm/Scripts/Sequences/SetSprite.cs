using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnitySequencerSystem;
using UnitySequencerSystem.Resolvers;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class SetSprite : ISequence
    {
        [SerializeReference, SubclassSelector]
        private ImageResolver target;

        [SerializeField]
        private Sprite sprite;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            target.Resolve(container).sprite = sprite;
            return default;
        }
    }
}
