namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Extensions
    {
        public static string ToContentAvailabilityName(this Define.TabType self)
        {
            return $"TabType.{self}";
        }
    }
}
