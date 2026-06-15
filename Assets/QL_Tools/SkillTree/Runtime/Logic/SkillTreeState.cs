using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillTree
{
    [Serializable]
    public class SkillTreeState : ISerializationCallbackReceiver
    {
        [Serializable]
        public struct NodeRank
        {
            public string NodeId;
            public int Rank;
        }

        [SerializeField] private string treeId;
        [SerializeField] private int version = 1;
        [SerializeField] private int availablePoints;
        [SerializeField] private List<NodeRank> ranks = new List<NodeRank>();

        [NonSerialized] private Dictionary<string, int> cache = new Dictionary<string, int>();

        public string TreeId => treeId;
        public int Version { get => version; set => version = value; }
        public int AvailablePoints => availablePoints;

        public SkillTreeState() { }

        public static SkillTreeState CreateFor(SkillTreeSO tree, int startingPoints = 0)
        {
            return new SkillTreeState
            {
                treeId = tree != null ? tree.TreeId : null,
                version = 1,
                availablePoints = Mathf.Max(0, startingPoints)
            };
        }

        public int GetRank(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return 0;
            return cache.TryGetValue(nodeId, out var rank) ? rank : 0;
        }

        public void SetRank(string nodeId, int rank)
        {
            if (string.IsNullOrEmpty(nodeId)) return;
            if (rank <= 0) cache.Remove(nodeId);
            else cache[nodeId] = rank;
        }

        public void AddPoints(int amount)
        {
            availablePoints = Mathf.Max(0, availablePoints + amount);
        }

        public bool TrySpendPoints(int amount)
        {
            if (amount < 0 || availablePoints < amount) return false;
            availablePoints -= amount;
            return true;
        }

        public void RefundAllPoints(int amount)
        {
            availablePoints += Mathf.Max(0, amount);
        }

        public void ClearRanks()
        {
            cache.Clear();
        }

        public IReadOnlyDictionary<string, int> Ranks => cache;

        public void OnBeforeSerialize()
        {
            ranks.Clear();
            foreach (var pair in cache)
            {
                if (pair.Value > 0) ranks.Add(new NodeRank { NodeId = pair.Key, Rank = pair.Value });
            }
        }

        public void OnAfterDeserialize()
        {
            cache = new Dictionary<string, int>(ranks.Count);
            foreach (var entry in ranks)
            {
                if (!string.IsNullOrEmpty(entry.NodeId)) cache[entry.NodeId] = entry.Rank;
            }
        }
    }
}
