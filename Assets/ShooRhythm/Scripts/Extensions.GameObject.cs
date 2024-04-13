using HK;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static void SetActiveIfNeed(this GameObject self, bool active)
        {
            if (self.activeSelf != active)
            {
                self.SetActive(active);
            }
        }
    }
}
