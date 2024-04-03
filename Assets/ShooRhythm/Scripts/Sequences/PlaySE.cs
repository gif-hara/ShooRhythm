using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;
using UnitySequencerSystem;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class PlaySE : ISequence
    {
        [SerializeField]
        private AudioClip audioClip;

        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            TinyServiceLocator.Resolve<AudioController>().PlaySE(audioClip);
            return UniTask.CompletedTask;
        }
    }
}
