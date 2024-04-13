using Cysharp.Threading.Tasks;
using HK;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SceneControllerGame : MonoBehaviour
    {
        async UniTask Start()
        {
            await TinyServiceLocator.Resolve<BootSystem>().IsReady;
            TinyServiceLocator.RegisterAsync(new GameData()).Forget();
        }
    }
}
