using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;
using UnitySequencerSystem;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Actor : MonoBehaviour
    {
        [SerializeField]
        private ScriptableSequences onClickAnimationSequences;

        [SerializeField]
        private Transform animationTarget;

        void Awake()
        {
            RegisterAsync().Forget();
        }

        public void OnClick()
        {
            var container = new Container();
            container.Register("AnimationTarget", animationTarget);
            var sequencer = new Sequencer(container, onClickAnimationSequences.Sequences);
            sequencer.PlayAsync(destroyCancellationToken).Forget();
        }

        private async UniTaskVoid RegisterAsync()
        {
            var actorManager = await TinyServiceLocator.ResolveAsync<ActorManager>();
            var p = transform.position;
            var key = new Vector2Int((int)p.x, (int)p.y);
            actorManager.AddActor(key, this);
            await UniTask.WaitUntilCanceled(destroyCancellationToken);
            actorManager.RemoveActor(key);
        }
    }
}
