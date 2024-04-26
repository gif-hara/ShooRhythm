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
    }
}
