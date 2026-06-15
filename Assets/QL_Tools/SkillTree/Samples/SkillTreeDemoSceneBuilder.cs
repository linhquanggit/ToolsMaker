#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkillTree.Samples
{
    public static class SkillTreeDemoSceneBuilder
    {
        private const string Dir = "Assets/QL_Tools/SkillTree/Samples/Generated";
        private const string ScenePath = Dir + "/SkillTreeDemo.unity";

        [MenuItem("Tools/SkillTree/Samples/Create Demo Scene")]
        public static void CreateDemoScene()
        {
            var tree = FindTree();
            if (tree == null)
            {
                EditorUtility.DisplayDialog("SkillTree",
                    "Chưa có SkillTreeSO nào. Chạy 'Tools > SkillTree > Samples > Create Rune Tree' trước.", "OK");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var playerGo = new GameObject("Player");
            var player = playerGo.AddComponent<DemoPlayerStats>();

            var demoGo = new GameObject("SkillTreeDemo");
            var demo = demoGo.AddComponent<SkillTreeViewDemo>();

            var so = new SerializedObject(demo);
            so.FindProperty("tree").objectReferenceValue = tree;
            so.FindProperty("startingPoints").intValue = 200000;
            so.FindProperty("player").objectReferenceValue = player;
            so.ApplyModifiedPropertiesWithoutUndo();

            EnsureFolder(Dir);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("SkillTree",
                $"Đã tạo demo scene:\n{ScenePath}\n\nTree: {tree.name}\nBấm Play để thử (click node → stats Player đổi bên phải).", "OK");
        }

        private static SkillTreeSO FindTree()
        {
            var guids = AssetDatabase.FindAssets("t:SkillTreeSO");
            SkillTreeSO first = null;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SkillTreeSO>(path);
                if (asset == null) continue;
                if (asset.name == "RuneTree") return asset;
                if (first == null) first = asset;
            }
            return first;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace("\\", "/");
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
