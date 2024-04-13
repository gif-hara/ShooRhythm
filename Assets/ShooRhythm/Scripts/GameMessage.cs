using R3;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameMessage
    {
        public static readonly Subject<Unit> RequestShowItemList = new();
    }
}
