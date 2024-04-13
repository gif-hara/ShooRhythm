using R3;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameMessage
    {
        public readonly Subject<Unit> RequestShowItemList = new();

        public readonly Subject<(int id, int count)> UpdatedItem = new();
    }
}
