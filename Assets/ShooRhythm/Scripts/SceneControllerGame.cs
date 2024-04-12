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
            TinyServiceLocator.RegisterAsync(new ActorManager()).Forget();

            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    var mouse = Mouse.current;
                    if (mouse.rightButton.wasPressedThisFrame)
                    {
                        lastMousePosition = mouse.position.ReadValue();
                    }
                    if (mouse.rightButton.isPressed)
                    {
                        var currentMousePosition = mouse.position.ReadValue();
                        var mouseDelta = currentMousePosition - lastMousePosition;
                        var cameraSize = controllerdCamera.orthographicSize;
                        var cameraAspect = controllerdCamera.aspect;
                        var cameraHeight = cameraSize * 2;
                        var cameraWidth = cameraHeight * cameraAspect;
                        var move = new Vector3(
                            mouseDelta.x / Screen.width * cameraWidth,
                            mouseDelta.y / Screen.height * cameraHeight,
                            0.0f
                        ) * mouseSensitivity;
                        controllerdCamera.transform.position -= move;
                        lastMousePosition = currentMousePosition;
                    }
                    var scroll = mouse.scroll.ReadValue();
                    if (scroll.y != 0)
                    {
                        controllerdCamera.orthographicSize = Mathf.Clamp(
                            controllerdCamera.orthographicSize - scroll.y * scrollSensitivity,
                            cameraSizeMin,
                            cameraSizeMax
                        );
                    }
                    if (mouse.leftButton.wasPressedThisFrame)
                    {
                        var mousePosition = mouse.position.ReadValue();
                        var position = controllerdCamera.ScreenToWorldPoint(mousePosition);
                        Debug.Log(position);
                        var key = new Vector2Int((int)position.x, (int)position.y);
                        TinyServiceLocator.Resolve<ActorManager>().TryGetActor(key)?.OnClick();
                    }
                })
                .RegisterTo(destroyCancellationToken);
        }
    }
}
