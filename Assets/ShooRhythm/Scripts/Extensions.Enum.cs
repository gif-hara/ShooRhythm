namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static string ToContentAvailableName(this Define.TabType self)
        {
            return $"TabType.{self}";
        }

        public static int GetEnhanceGroupId(this Define.EnhanceType self, int level)
        {
            return 10000 + (int)self * 100 + level;
        }
    }
}
