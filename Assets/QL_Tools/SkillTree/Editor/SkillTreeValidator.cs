using System.Collections.Generic;
using System.Text;
using UnityEditor;

namespace SkillTree.Editor
{
    public static class SkillTreeValidator
    {
        public static List<string> Validate(SkillTreeSO tree)
        {
            var issues = new List<string>();
            if (tree == null)
            {
                issues.Add("Tree is null.");
                return issues;
            }

            var ids = new HashSet<string>();
            foreach (var node in tree.Nodes)
            {
                if (node == null)
                {
                    issues.Add("Null node entry in tree.");
                    continue;
                }
                if (string.IsNullOrEmpty(node.Id)) issues.Add($"Node '{node.name}' has an empty Id.");
                else if (!ids.Add(node.Id)) issues.Add($"Duplicate Id '{node.Id}' on node '{node.name}'.");
            }

            if (HasCycle(tree, out var cycleName)) issues.Add($"Prerequisite cycle detected involving '{cycleName}'.");

            if (tree.RootNodes.Count > 0)
            {
                var reachable = ComputeReachable(tree);
                foreach (var node in tree.Nodes)
                {
                    if (node == null || reachable.Contains(node)) continue;
                    issues.Add($"Node '{node.DisplayName}' is unreachable from root nodes.");
                }
            }

            return issues;
        }

        public static void ValidateAndReport(SkillTreeSO tree)
        {
            var issues = Validate(tree);
            if (issues.Count == 0)
            {
                EditorUtility.DisplayDialog("Skill Tree Validation", "No issues found.", "OK");
                return;
            }

            var sb = new StringBuilder();
            foreach (var issue in issues) sb.AppendLine("• " + issue);
            EditorUtility.DisplayDialog("Skill Tree Validation", sb.ToString(), "OK");
        }

        private static bool HasCycle(SkillTreeSO tree, out string nodeName)
        {
            nodeName = null;
            var visited = new HashSet<SkillNodeSO>();
            var stack = new HashSet<SkillNodeSO>();
            foreach (var node in tree.Nodes)
            {
                if (node == null) continue;
                if (Visit(node, visited, stack, out nodeName)) return true;
            }
            return false;
        }

        private static bool Visit(SkillNodeSO node, HashSet<SkillNodeSO> visited, HashSet<SkillNodeSO> stack, out string nodeName)
        {
            nodeName = null;
            if (stack.Contains(node))
            {
                nodeName = node.DisplayName;
                return true;
            }
            if (!visited.Add(node)) return false;

            stack.Add(node);
            foreach (var group in node.Prerequisites)
            {
                if (group == null) continue;
                foreach (var prereq in group.Nodes)
                {
                    if (prereq == null) continue;
                    if (Visit(prereq, visited, stack, out nodeName)) return true;
                }
            }
            stack.Remove(node);
            return false;
        }

        private static HashSet<SkillNodeSO> ComputeReachable(SkillTreeSO tree)
        {
            var reachable = new HashSet<SkillNodeSO>();
            foreach (var root in tree.RootNodes)
            {
                if (root != null) reachable.Add(root);
            }

            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var node in tree.Nodes)
                {
                    if (node == null || reachable.Contains(node) || node.Prerequisites.Count == 0) continue;
                    foreach (var group in node.Prerequisites)
                    {
                        if (group == null) continue;
                        bool linked = false;
                        foreach (var prereq in group.Nodes)
                        {
                            if (prereq != null && reachable.Contains(prereq))
                            {
                                linked = true;
                                break;
                            }
                        }
                        if (linked)
                        {
                            reachable.Add(node);
                            changed = true;
                            break;
                        }
                    }
                }
            }
            return reachable;
        }
    }
}
