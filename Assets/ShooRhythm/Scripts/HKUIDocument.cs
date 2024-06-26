using System;
using System.Collections.Generic;
using UnityEngine;

namespace HK
{
    /// <summary>
    /// Represents a UI document that contains a collection of UI elements.
    /// </summary>
    public class HKUIDocument : MonoBehaviour
    {
        [SerializeField]
        private Element[] elements;

        private readonly Dictionary<string, GameObject> elementMap = new();

        private readonly Dictionary<GameObject, Dictionary<Type, Component>> componentMap = new();

        /// <summary>
        /// Retrieves a component of type T from the UI element with the specified name.
        /// </summary>
        /// <typeparam name="T">The type of the component to retrieve.</typeparam>
        /// <param name="name">The name of the UI element.</param>
        /// <returns>The component of type T if found; otherwise, null.</returns>
        public T Q<T>(string name) where T : Component
        {
            var e = Q(name);
            if (e == null)
            {
                return null;
            }

            if (!componentMap.TryGetValue(e, out var c))
            {
                c = new Dictionary<Type, Component>();
                componentMap[e] = c;
            }

            if (c.TryGetValue(typeof(T), out var component))
            {
                return (T)component;
            }

            if (e.TryGetComponent<T>(out var newComponent))
            {
                c[typeof(T)] = newComponent;
                return newComponent;
            }

            Debug.LogError($"Component not found: {typeof(T)} on object {this.name}.{name}");
            return null;
        }
        
        public (T1, T2) Q<T1, T2>(string name) where T1 : Component where T2 : Component
        {
            return (Q<T1>(name), Q<T2>(name));
        }
        
        public (T1, T2, T3) Q<T1, T2, T3>(string name) where T1 : Component where T2 : Component where T3 : Component
        {
            return (Q<T1>(name), Q<T2>(name), Q<T3>(name));
        }
        
        public (T1, T2, T3, T4) Q<T1, T2, T3, T4>(string name) where T1 : Component where T2 : Component where T3 : Component where T4 : Component
        {
            return (Q<T1>(name), Q<T2>(name), Q<T3>(name), Q<T4>(name));
        }

        /// <summary>
        /// Retrieves the UI element with the specified name.
        /// </summary>
        /// <param name="name">The name of the UI element.</param>
        /// <returns>The UI element if found; otherwise, null.</returns>
        public GameObject Q(string name)
        {
            if (elementMap.Count == 0)
            {
                foreach (var element in elements)
                {
                    elementMap[element.Name] = element.Document;
                }
            }

            if (elementMap.TryGetValue(name, out var e))
            {
                return e;
            }
            else
            {
                Debug.LogError($"Element not found: {this.name}.{name}");
                return null;
            }
        }

        [Serializable]
        public class Element
        {
            [SerializeField]
            private GameObject document;

            [SerializeField]
            private string overrideName;

            public GameObject Document => document;

            public string Name => string.IsNullOrEmpty(overrideName) ? document.name : overrideName;
        }
    }
}