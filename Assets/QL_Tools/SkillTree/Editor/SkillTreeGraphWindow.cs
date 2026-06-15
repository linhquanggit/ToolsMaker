using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTree.Editor
{
    public class SkillTreeGraphWindow : EditorWindow
    {
        private SkillNodeGraphView graphView;
        private SkillTreeSO tree;
        private ObjectField treeField;

        [MenuItem("Tools/SkillTree/Graph Editor")]
        public static void Open()
        {
            var window = GetWindow<SkillTreeGraphWindow>();
            window.titleContent = new GUIContent("Skill Tree");
            window.minSize = new Vector2(640f, 400f);
        }

        public static void OpenFor(SkillTreeSO target)
        {
            Open();
            GetWindow<SkillTreeGraphWindow>().SetTree(target);
        }

        public void SetTree(SkillTreeSO target)
        {
            tree = target;
            treeField?.SetValueWithoutNotify(target);
            graphView?.SetTree(target);
        }

        private void CreateGUI()
        {
            var toolbar = new Toolbar();

            treeField = new ObjectField("Tree") { objectType = typeof(SkillTreeSO), allowSceneObjects = false };
            treeField.RegisterValueChangedCallback(evt => SetTree(evt.newValue as SkillTreeSO));
            toolbar.Add(treeField);

            toolbar.Add(new ToolbarButton(() => graphView?.CreateNode(Vector2.zero)) { text = "Add Node" });
            toolbar.Add(new ToolbarButton(() =>
            {
                if (tree != null) SkillTreeValidator.ValidateAndReport(tree);
            })
            { text = "Validate" });

            rootVisualElement.Add(toolbar);

            graphView = new SkillNodeGraphView();
            rootVisualElement.Add(graphView);

            if (tree != null) SetTree(tree);
        }
    }
}
