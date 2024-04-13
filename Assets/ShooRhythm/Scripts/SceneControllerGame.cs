using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnitySequencerSystem;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SceneControllerGame : MonoBehaviour
    {
        [SerializeField]
        private Camera controllerdCamera;

        [SerializeField]
        private float mouseSensitivity = 0.1f;

        [SerializeField]
        private float scrollSensitivity = 0.1f;

        [SerializeField]
        private float cameraSizeMin = 1.0f;

        [SerializeField]
        private float cameraSizeMax = 10.0f;

        private Vector2 lastMousePosition;


        async UniTask Start()
        {
            await TinyServiceLocator.Resolve<BootSystem>().IsReady;
            TinyServiceLocator.RegisterAsync(new GameData()).Forget();
        }
    }
}
