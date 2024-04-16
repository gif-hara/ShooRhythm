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
    public sealed class SetColor : ISequence
    {
        [SerializeReference, SubclassSelector]
        private GraphicResolver graphicResolver;
        
        [SerializeField]
        private Color color;
        
        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var graphic = graphicResolver.Resolve(container);
            graphic.color = color;
            return UniTask.CompletedTask;
        }
    }
}
