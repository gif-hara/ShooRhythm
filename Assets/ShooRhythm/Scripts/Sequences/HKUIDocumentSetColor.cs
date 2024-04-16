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
    public sealed class HKUIDocumentSetColor : ISequence
    {
        [SerializeField]
        private string documentName;

        [SerializeField]
        private string graphicName;
        
        [SerializeField]
        private Color color;
        
        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            var document = container.Resolve<HKUIDocument>(documentName);
            document.Q<Graphic>(graphicName).color = color;
            return UniTask.CompletedTask;
        }
    }
}
