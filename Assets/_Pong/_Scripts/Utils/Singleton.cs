using System;
using UnityEngine;

namespace NoMossStudios.Utilities
{
    /// <summary>
    /// Singleton class
    /// </summary>
    /// <typeparam name="T">Type of the singleton</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;

        /// <summary>
        /// The static reference to the instance
        /// </summary>
        public static T Instance
        {
            get => _instance;
            protected set
            {
                _instance = value;
                OnInstanceSet?.Invoke(_instance);
            }
        }

        /// <summary>
        /// Gets whether an instance of this singleton exists
        /// </summary>
        public static bool InstanceExists => _instance != null;

        public static event Action<Singleton<T>> OnInstanceSet;

        /// <summary>
        /// Awake method to associate singleton with instance
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = (T)this;
                OnInstanceSet?.Invoke(this);
            }
        }

        /// <summary>
        /// OnDestroy method to clear singleton association
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}