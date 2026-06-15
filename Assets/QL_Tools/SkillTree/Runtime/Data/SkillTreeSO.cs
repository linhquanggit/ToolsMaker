using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillTree
{
    [CreateAssetMenu(fileName = "SkillTree", menuName = "SkillTree/Skill Tree")]
    public class SkillTreeSO : ScriptableObject
    {
        [SerializeField] private string treeId;
        [SerializeField] private string currencyId = "skillpoint";
        [SerializeField] private List<SkillNodeSO> nodes = new List<SkillNodeSO>();
        [SerializeField] private List<SkillNodeSO> rootNodes = new List<SkillNodeSO>();

        private Dictionary<string, SkillNodeSO> lookup;

        public string TreeId => treeId;
        public string CurrencyId => currencyId;
        public IReadOnlyList<SkillNodeSO> Nodes => nodes;
        public IReadOnlyList<SkillNodeSO> RootNodes => rootNodes;

        public SkillNodeSO FindById(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return null;
            EnsureLookup();
            return lookup.TryGetValue(nodeId, out var node) ? node : null;
        }

        public bool IsRoot(SkillNodeSO node)
        {
            return node != null && rootNodes != null && rootNodes.Contains(node);
        }

        public void InvalidateLookup() => lookup = null;

        private void EnsureLookup()
        {
            if (lookup != null) return;
            lookup = new Dictionary<string, SkillNodeSO>(nodes.Count);
            foreach (var node in nodes)
            {
                if (node == null || string.IsNullOrEmpty(node.Id)) continue;
                lookup[node.Id] = node;
            }
        }

#if UNITY_EDITOR
        public void Editor_SetMeta(string treeId, string currencyId)
        {
            this.treeId = treeId;
            this.currencyId = currencyId;
        }

        public void Editor_AddNode(SkillNodeSO node)
        {
            if (node == null || nodes.Contains(node)) return;
            nodes.Add(node);
            InvalidateLookup();
        }

        public void Editor_RemoveNode(SkillNodeSO node)
        {
            nodes.Remove(node);
            rootNodes.Remove(node);
            InvalidateLookup();
        }

        public void Editor_SetRoot(SkillNodeSO node, bool isRoot)
        {
            if (node == null) return;
            if (isRoot && !rootNodes.Contains(node)) rootNodes.Add(node);
            else if (!isRoot) rootNodes.Remove(node);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(treeId)) treeId = Guid.NewGuid().ToString("N");
            InvalidateLookup();
        }
#endif
    }
}
