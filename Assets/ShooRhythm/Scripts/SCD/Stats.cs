using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCD
{
    /// <summary>
    /// 統計データ
    /// </summary>
    public sealed class Stats
    {
        private readonly Dictionary<string, Record> records = new();

        /// <summary>
        /// 変更があった際に呼び出されるイベント
        /// </summary>
        public event Action<Record> OnChanged;

        /// <summary>
        /// 統計データを設定する
        /// </summary>
        public void Set(string name, int value)
        {
            if (records.TryGetValue(name, out var record))
            {
                record.Value = value;
            }
            else
            {
                record = new Record(name, value);
                records.Add(name, record);
            }
            OnChanged?.Invoke(record);
        }

        /// <summary>
        /// 統計データを加算する
        /// </summary>
        public void Add(string name, int value)
        {
            if (records.TryGetValue(name, out var record))
            {
                record.Value += value;
            }
            else
            {
                record = new Record(name, value);
                records.Add(name, record);
            }
            OnChanged?.Invoke(record);
        }

        /// <summary>
        /// 統計データを取得する
        /// </summary>
        public int Get(string name)
        {
            return records.TryGetValue(name, out var record) ? record.Value : 0;
        }

        /// <summary>
        /// 統計データが存在するかどうかを取得する
        /// </summary>
        public bool Contains(string name)
        {
            return records.ContainsKey(name);
        }

        [Serializable]
        public class Record
        {
            /// <summary>
            /// 名前
            /// </summary>
            [field: SerializeField]
            public string Name { get; private set; }

            /// <summary>
            /// 値
            /// </summary>
            [field: SerializeField]
            public int Value { get; set; }

            public Record(string name, int value)
            {
                Name = name;
                Value = value;
            }
        }
    }
}
