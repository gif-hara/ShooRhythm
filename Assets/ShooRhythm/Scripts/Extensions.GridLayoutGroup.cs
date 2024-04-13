using UnityEngine;
using UnityEngine.UI;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static void SetConstraintCount(this GridLayoutGroup self)
        {
            if (self.constraint == GridLayoutGroup.Constraint.Flexible)
            {
                return;
            }
            Canvas.ForceUpdateCanvases();
            var max = self.constraint == GridLayoutGroup.Constraint.FixedRowCount
                ? Mathf.FloorToInt(((RectTransform)self.transform).rect.height - self.padding.top - self.padding.bottom - self.spacing.y)
                : Mathf.FloorToInt(((RectTransform)self.transform).rect.width - self.padding.left - self.padding.right - self.spacing.x);
            var cellSize = self.constraint == GridLayoutGroup.Constraint.FixedRowCount
                ? Mathf.FloorToInt(self.cellSize.y + self.spacing.y)
                : Mathf.FloorToInt(self.cellSize.x + self.spacing.x);
            self.constraintCount = max / cellSize;
        }
    }
}
