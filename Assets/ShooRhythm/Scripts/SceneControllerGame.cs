using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;
using UnitySequencerSystem;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SceneControllerGame : MonoBehaviour
    {
        [SerializeField]
        private Actor playerPrefab;

        [SerializeField]
        private ScriptableSequences scriptableSequences;


        async UniTask Start()
        {
            await TinyServiceLocator.Resolve<BootSystem>().IsReady;

            var player = Instantiate(playerPrefab);
            var inputActions = new InputActions();
            inputActions.Enable();
            inputActions.InGame.Fire.OnPerformedAsObservable()
                .Subscribe(_ =>
                {
                    var container = new Container();
                    container.Register("Actor", player);
                    container.Register("Actor", player.transform);
                    var sequencer = new Sequencer(container, scriptableSequences.Sequences);
                    sequencer.PlayAsync(destroyCancellationToken).Forget();
                })
                .RegisterTo(destroyCancellationToken);
        }
    }
}
