using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static async UniTaskVoid SetIconAsync(this Image self, UniTask<Sprite> loadSpriteTask, Action<Sprite> onCompleted = null)
        {
            self.sprite = null;
            self.enabled = false;
            var sprite = await loadSpriteTask;
            if(self != null)
            {
                self.sprite = sprite;
                self.enabled = sprite != null;
                onCompleted?.Invoke(sprite);
            }
        }
    }
}
