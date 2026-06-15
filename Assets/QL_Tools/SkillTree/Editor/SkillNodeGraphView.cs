using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTree.Editor
{
    public class SkillNodeGraphView : GraphView
    {
        private SkillTreeSO tree;
        private readonly Dictionary<SkillNodeSO, SkillNodeView> views = new Dictionary<SkillNodeSO, SkillNodeView>();

        public SkillNodeGraphView()
        {
            style.flexGrow = 1;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            graphViewChanged = OnGraphChanged;
        }

        public void SetTree(SkillTreeSO target)
        {
            tree = target;
            Rebuild();
        }

        public void Rebuild()
        {
            graphViewChanged = null;
            DeleteElements(graphElements.ToList());
            views.Clear();

            if (tree != null)
            {
                foreach (var node in tree.Nodes)
                {
                    if (node != null) CreateNodeView(node);
                }
                foreach (var node in tree.Nodes)
                {
                    if (node != null) BuildEdgesFor(node);
                }
            }
            graphViewChanged = OnGraphChanged;
        }

        public void CreateNode(Vector2 position)
        {
            if (tree == null) return;

            var node = ScriptableObject.CreateInstance<SkillNodeSO>();
            node.name = "SkillNode";
            node.Editor_EnsureId();
            node.Editor_SetGraphPosition(position);

            Undo.RegisterCreatedObjectUndo(node, "Create Skill Node");
            AssetDatabase.AddObjectToAsset(node, tree);
            Undo.RecordObject(tree, "Add Skill Node");
            tree.Editor_AddNode(node);
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();

            CreateNodeView(node);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(p => p.direction != startPort.direction && p.node != startPort.node).ToList();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (tree != null)
            {
                Vector2 pos = contentViewContainer.WorldToLocal(evt.localMousePosition);
                evt.menu.AppendAction("Add Skill Node", _ => CreateNode(pos));
            }
            base.BuildContextualMenu(evt);
        }

        private SkillNodeView CreateNodeView(SkillNodeSO node)
        {
            var view = new SkillNodeView(node);
            views[node] = view;
            AddElement(view);
            return view;
        }

        private void BuildEdgesFor(SkillNodeSO node)
        {
            if (!views.TryGetValue(node, out var target)) return;
            foreach (var group in node.Prerequisites)
            {
                if (group == null) continue;
                foreach (var prereq in group.Nodes)
                {
                    if (prereq == null || !views.TryGetValue(prereq, out var source)) continue;
                    AddElement(source.Output.ConnectTo(target.Input));
                }
            }
        }

        private GraphViewChange OnGraphChanged(GraphViewChange change)
        {
            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    if (element is SkillNodeView view)
                    {
                        Undo.RecordObject(view.Data, "Move Skill Node");
                        view.Data.Editor_SetGraphPosition(view.GetPosition().position);
                        EditorUtility.SetDirty(view.Data);
                    }
                }
            }

            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    if (edge.output.node is SkillNodeView source && edge.input.node is SkillNodeView target)
                    {
                        Undo.RecordObject(target.Data, "Add Prerequisite");
                        target.Data.Editor_AddPrerequisite(source.Data);
                        EditorUtility.SetDirty(target.Data);
                    }
                }
            }

            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is Edge edge)
                    {
                        if (edge.output?.node is SkillNodeView source && edge.input?.node is SkillNodeView target)
                        {
                            Undo.RecordObject(target.Data, "Remove Prerequisite");
                            target.Data.Editor_RemovePrerequisite(source.Data);
                            EditorUtility.SetDirty(target.Data);
                        }
                    }
                    else if (element is SkillNodeView view)
                    {
                        DeleteNodeAsset(view.Data);
                    }
                }
            }

            return change;
        }

        private void DeleteNodeAsset(SkillNodeSO node)
        {
            if (tree == null || node == null) return;

            Undo.RecordObject(tree, "Delete Skill Node");
            tree.Editor_RemoveNode(node);
            foreach (var other in tree.Nodes)
            {
                if (other == null) continue;
                Undo.RecordObject(other, "Delete Skill Node");
                other.Editor_RemovePrerequisite(node);
                EditorUtility.SetDirty(other);
            }
            EditorUtility.SetDirty(tree);
            views.Remove(node);
            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }
    }
}
