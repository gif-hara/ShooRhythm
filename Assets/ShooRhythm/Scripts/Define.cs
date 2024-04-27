namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    public static class Define
    {
        public enum CollectionType
        {
            None = 0,
            Collection = 1,
            Production = 2,
            Battle = 3,
            Fishing = 4,
            Farm = 5,
        }

        public enum TabType
        {
            None = 0,
            Items = 1,
            Collections = 2,
            Productions = 3,
            Quests = 4,
            Equipment = 5,
            RiverFishing = 6,
            SeaFishing = 7,
            Farm = 8,
            Meadow = 9,
            Grassland = 10,
            Meal = 11,
        }

        public enum NotificationType
        {
            Positive = 0,
            Negative = 1,
        }

        public enum DungeonType
        {
            Grassland = 1,
            Wetland = 2,
        }

        public enum AttackResultType
        {
            Unknown = 0,
            Hit = 1,
            Defeat = 2,
        }

        public enum FishingType
        {
            River = 1,
            Sea = 2,
        }

        public enum ProcessResultType
        {
            Unknown = 0,
            Success = 1,

            /// <summary>
            /// アイテムが足りない
            /// </summary>
            NotEnoughItem = 2,

            /// <summary>
            /// 農場のタイマーが進行中
            /// </summary>
            FarmTimerInProgress = 3,
        }

        public const int MachineSlotCount = 3;
    }
}
