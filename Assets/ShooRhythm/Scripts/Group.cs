using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShooRhythm
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public abstract class Group<TKey, TValue>
    {
        [Serializable]
        public class Element
        {
            public TKey Key;

            public List<TValue> Value;
        }

        protected Func<TValue, TKey> keySelector;

        [SerializeField]
        private List<Element> list = new();

        public Group(Func<TValue, TKey> keySelector)
        {
            this.keySelector = keySelector;
        }

        public void Set(IEnumerable<TValue> values)
        {
            list.Clear();
            foreach (var value in values)
            {
                var key = keySelector(value);
                var element = list.Find(x => x.Key.Equals(key));
                if (element == null)
                {
                    element = new Element { Key = key, Value = new List<TValue>() };
                    list.Add(element);
                }
                element.Value.Add(value);
            }
        }
    }
}
