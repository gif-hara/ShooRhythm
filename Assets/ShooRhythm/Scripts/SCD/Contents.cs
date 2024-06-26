using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCD
{
    /// <summary>
    /// ゲームを構成するコンテンツ
    /// </summary>
    [Serializable]
    public class Contents
    {
        [SerializeField]
        private Record[] records = new Record[0];
        public Record[] Records => records;

        public Contents(IEnumerable<Record> records)
        {
            this.records = records.ToArray();
        }

        /// <summary>
        /// 利用可能なコンテンツを取得する
        /// </summary>
        public IEnumerable<Record> GetAvailables(Stats stats)
        {
            foreach (var i in records)
            {
                if (i.IsAvailable(stats) && !i.BeIgnored(stats))
                {
                    yield return i;
                }
            }
        }

        public Record Get(string name)
        {
            return records.First(x => x.Name == name);
        }

        [Serializable]
        public class Record
        {
            /// <summary>
            /// このコンテンツの名前
            /// </summary>
            [field: SerializeField]
            public string Name { get; private set; }

            /// <summary>
            /// 開始するために必要な統計データ
            /// </summary>
            [field: SerializeField]
            public List<Stats.Record> Required { get; private set; }

            /// <summary>
            /// このコンテンツが無視される統計データ
            /// </summary>
            /// <remarks>
            /// このリストにヒットした場合は有効なコンテンツではないと判定されます
            /// </remarks>
            [field: SerializeField]
            public List<Stats.Record> Ignore { get; private set; }

            /// <summary>
            /// 完了するために必要な統計データ
            /// </summary>
            [field: SerializeField]
            public List<Stats.Record> Conditions { get; private set; }

            /// <summary>
            /// 完了した際の報酬となる統計データ
            /// </summary>
            [field: SerializeField]
            public List<Stats.Record> Rewards { get; private set; }

            public Record(
                string name,
                List<Stats.Record> required,
                List<Stats.Record> ignore,
                List<Stats.Record> conditions,
                List<Stats.Record> rewards
                )
            {
                Name = name;
                Required = required;
                Ignore = ignore;
                Conditions = conditions;
                Rewards = rewards;
            }

            /// <summary>
            /// 利用可能かどうかを判定する
            /// </summary>
            public bool IsAvailable(Stats stats)
            {
                foreach (var record in Required)
                {
                    if (stats.Get(record.Name) < record.Value)
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// 無視されるかどうかを判定する
            /// </summary>
            public bool BeIgnored(Stats stats)
            {
                foreach (var record in Ignore)
                {
                    if (stats.Get(record.Name) >= record.Value)
                    {
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// 完了したかどうかを判定する
            /// </summary>
            public bool IsCompleted(Stats stats)
            {
                foreach (var record in Conditions)
                {
                    if (stats.Get(record.Name) < record.Value)
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// 報酬を適用する
            /// </summary>
            public void ApplyRewards(Stats stats)
            {
                foreach (var record in Rewards)
                {
                    stats.Add(record.Name, record.Value);
                }
            }
        }
    }
}
