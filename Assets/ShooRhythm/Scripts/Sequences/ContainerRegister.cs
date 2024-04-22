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
    public abstract class ContainerRegister<T> : ISequence
    {
        [SerializeField]
        private T target;

        [SerializeField]
        private string key;
        
        [SerializeField]
        private bool registerOrReplace;
        
        public UniTask PlayAsync(Container container, CancellationToken cancellationToken)
        {
            if (registerOrReplace)
            {
                container.RegisterOrReplace(key, target);
            }
            else
            {
                container.Register(key, target);
            }
            return UniTask.CompletedTask;
        }
    }
    
    [Serializable]
    public sealed class ContainerRegisterGameObject : ContainerRegister<GameObject> { }
}
