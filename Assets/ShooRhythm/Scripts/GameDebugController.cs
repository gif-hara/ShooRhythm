using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine.InputSystem;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameDebugController
    {
        public void Begin(CancellationToken cancellationToken)
        {
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            var gameController = TinyServiceLocator.Resolve<GameController>();
            Observable.EveryUpdate(cancellationToken)
                .Subscribe(_ =>
                {
                    if (Keyboard.current.f1Key.wasPressedThisFrame)
                    {
                        foreach (var item in masterData.Items.List)
                        {
                            gameController.SetStatsAsync($"Item.{item.Id}", 99).Forget();
                        }
                    }
                })
                .RegisterTo(cancellationToken);
        }
    }
}
