using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameDebugController
    {
        public static void Begin(CancellationToken cancellationToken)
        {
            var masterData = TinyServiceLocator.Resolve<MasterData>();
            var gameController = TinyServiceLocator.Resolve<GameController>();
            Observable.EveryUpdate(cancellationToken)
                .Subscribe(_ =>
                {
                    var gameDebugData = TinyServiceLocator.Resolve<GameDebugData>();
                    if (Keyboard.current.f1Key.wasPressedThisFrame)
                    {
                        foreach (var item in masterData.Items.List)
                        {
                            gameController.DebugAddItemAsync(item.Id, 99).Forget();
                        }
                        Debug.Log("[DEBUG] Add All Items");
                    }
                    if (Keyboard.current.f2Key.wasPressedThisFrame)
                    {
                        gameController.AddProductMachine();
                        Debug.Log("[DEBUG] AddProductMachine");
                    }
                    if (Keyboard.current.f3Key.wasPressedThisFrame)
                    {
                        gameController.AddFarmData();
                        Debug.Log("[DEBUG] AddFarmData");
                    }
                    if (Keyboard.current.f4Key.wasPressedThisFrame)
                    {
                        gameDebugData.IgnoreCoolDown = !gameDebugData.IgnoreCoolDown;
                        Debug.Log($"[DEBUG] IgnoreCoolDown: {gameDebugData.IgnoreCoolDown}");
                    }
                })
                .RegisterTo(cancellationToken);
        }
    }
}
