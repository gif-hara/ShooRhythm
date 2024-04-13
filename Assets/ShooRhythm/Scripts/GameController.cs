using HK;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameController
    {
        public void AddStats(string name, int value)
        {
            TinyServiceLocator.Resolve<GameData>().Stats.Add(name, value);
        }

        public void SetStats(string name, int value)
        {
            TinyServiceLocator.Resolve<GameData>().Stats.Set(name, value);
        }
    }
}
