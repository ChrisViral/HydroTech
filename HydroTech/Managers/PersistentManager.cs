using System;
using System.Collections.Generic;
using UnityEngine;

namespace HydroTech.Managers
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class PersistentManager : MonoBehaviour
    {
        #region Instance
        public static PersistentManager Instance { get; private set; }
        #endregion

        #region Static fields
        private static readonly Dictionary<string, Dictionary<Type, ConfigNode>> modulesNodes = new Dictionary<string, Dictionary<Type, ConfigNode>>();
        #endregion

        #region Methods
        /// <summary>
        /// Stores a ConfigNode value in a persistent dictionary, sorted by Part name and PartModule time
        /// </summary>
        /// <typeparam name="T">PartModule type</typeparam>
        /// <param name="partName">Part name to use as Key</param>
        /// <param name="node">ConfigNode to store</param>
        public void AddNode<T>(string partName, ConfigNode node) where T : PartModule
        {
            Dictionary<Type, ConfigNode> dict;
            if (!modulesNodes.TryGetValue(partName, out dict))
            {
                dict = new Dictionary<Type, ConfigNode>();
                modulesNodes.Add(partName, dict);
            }
            Type type = typeof(T);
            if (!dict.ContainsKey(type))
            {
                dict.Add(type, node);
            }
        }

        /// <summary>
        /// Retreives a ConfigNode for the given PartModule type and Part name
        /// </summary>
        /// <typeparam name="T">PartModule type</typeparam>
        /// <param name="partName">Part name to get the node for</param>
        /// <param name="node">ConfigNode associated to the part and PartModule type</param>
        public bool TryGetNode<T>(string partName, ref ConfigNode node) where T : PartModule
        {
            Dictionary<Type, ConfigNode> dict;
            if (modulesNodes.TryGetValue(partName, out dict))
            {
                ConfigNode cfg;
                if (dict.TryGetValue(typeof(T), out cfg))
                {
                    node = cfg;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else { Destroy(this); }
        }
        #endregion
    }
}
