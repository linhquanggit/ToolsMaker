using UnityEditor;
using UnityEngine;

namespace SkillTree.Editor
{
    public class SkillTreeTutorialWindow : EditorWindow
    {
        private const string IntegrationCode =
@"// 1. Context — implement ISkillEffectContext, or use the built-in default
var context = new DictionaryStatContext();
var storage = new JsonFileStorage();

// 2. State — new, or loaded from disk
var state = storage.Exists(tree.TreeId)
    ? storage.Load(tree.TreeId)
    : SkillTreeState.CreateFor(tree, startingPoints: 10);

// 3. Controller — query + commands + events
var controller = new SkillTreeController(tree, state, context);
controller.ReapplyAllEffects();                       // reapply after load
controller.Events.OnNodeUnlocked += (node, rank) => { /* update UI */ };

if (controller.TryUnlock(nodeId) == UnlockResult.Success)
    storage.Save(controller.State);";

        private Vector2 scroll;
        private GUIStyle bodyStyle;
        private GUIStyle codeStyle;

        [MenuItem("Tools/SkillTree/Tutorial", priority = 0)]
        public static void Open()
        {
            var window = GetWindow<SkillTreeTutorialWindow>();
            window.titleContent = new GUIContent("SkillTree Tutorial");
            window.minSize = new Vector2(540f, 640f);
        }

        private void OnGUI()
        {
            EnsureStyles();
            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.Space(4f);
            GUILayout.Label("SkillTree — Hướng dẫn sử dụng", EditorStyles.largeLabel);
            Body("Pack skill tree data-driven, tách rời, tái sử dụng nhiều nơi. Một định nghĩa tree (asset) → nhiều instance (1 global tree + N hero tree). Ba tầng tách biệt: Definition (SO) ≠ State (per-owner) ≠ Effect (qua interface).");

            QuickActions();

            Header("Bước 1 — Tạo & vẽ tree (GraphView)");
            Body("1) Project: chuột phải > Create > SkillTree > Skill Tree (hoặc nút 'Tạo SkillTree mới' ở trên).\n" +
                 "2) Mở Tools > SkillTree > Graph Editor, kéo asset vào ô Tree.\n" +
                 "3) Nút 'Add Node' (hoặc chuột phải > Add Skill Node) để thêm node — mỗi node là sub-asset của tree.\n" +
                 "4) Chọn node ở Project, set Inspector: displayName, nodeType (Active/Passive), maxRank, cost (CostCurve).\n" +
                 "5) Nối port Unlocks → Requires giữa 2 node để tạo prerequisite.\n" +
                 "6) Kéo node gốc vào list rootNodes của tree.\n" +
                 "7) Nút 'Validate' bắt cycle / Id trùng / node không tới được từ root.");

            Header("Bước 2 — Gắn hiệu ứng cho node");
            Body("Tạo effect asset: Create > SkillTree > Effects > (Stat Modifier | Unlock Flag | Grant Ability), set giá trị, rồi kéo vào list 'effects' của node.\n" +
                 "• Stat Modifier: statId + valuePerRank + kiểu (Additive/Multiplicative/Override).\n" +
                 "• Unlock Flag: bật/tắt một cờ (vd 'chest.autoOpen').\n" +
                 "• Grant Ability: cấp một ability theo rank (vd 'knight.piercing_thrust').");

            Header("Bước 3 — Tích hợp runtime (3 dòng)");
            Body("Pack KHÔNG biết hệ stat của game — mọi hiệu ứng đi qua ISkillEffectContext. Dùng sẵn DictionaryStatContext hoặc tự implement.");
            CodeBox(IntegrationCode);

            Header("UI mẫu (test trực quan)");
            Body("Add component SkillTreeViewDemo lên 1 GameObject, gán tree + startingPoints, Play. UI tự dựng từ data: click node = unlock/rank-up · kéo nền = pan · lăn chuột = zoom. Màu: vàng=mở được, xanh teal=có rank, xanh lá=MAX, cam=thiếu điểm, xám=khóa.");

            Header("Mở rộng (không sửa core)");
            Body("• Hành vi node mới → subclass SkillEffectSO.\n" +
                 "• Kiểu chi phí mới → thêm case trong CostCurve.\n" +
                 "• Luật prereq mới → mở rộng PrerequisiteRule (All/Any/NofM).\n" +
                 "• Backend save mới → implement ISkillTreeStorage.\n" +
                 "• Gắn hệ stat khác → implement ISkillEffectContext.\n" +
                 "• Nhiều tree/instance → thêm asset + state.");

            EditorGUILayout.Space(8f);
            EditorGUILayout.EndScrollView();
        }

        private void QuickActions()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Thao tác nhanh", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Tạo SkillTree mới", GUILayout.Height(26f))) CreateNewTree();
            if (GUILayout.Button("Mở Graph Editor", GUILayout.Height(26f))) SkillTreeGraphWindow.Open();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Sinh Rune Tree mẫu", GUILayout.Height(26f))) SkillTreeSampleGenerator.CreateRuneTree();
            if (GUILayout.Button("Sinh Knight Tree mẫu", GUILayout.Height(26f))) SkillTreeSampleGenerator.CreateKnightTree();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private static void CreateNewTree()
        {
            var tree = CreateInstance<SkillTreeSO>();
            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/NewSkillTree.asset");
            AssetDatabase.CreateAsset(tree, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = tree;
            EditorGUIUtility.PingObject(tree);
            SkillTreeGraphWindow.OpenFor(tree);
        }

        private void Header(string text)
        {
            EditorGUILayout.Space(10f);
            GUILayout.Label(text, EditorStyles.boldLabel);
        }

        private void Body(string text)
        {
            EditorGUILayout.LabelField(text, bodyStyle);
        }

        private void CodeBox(string code)
        {
            EditorGUILayout.SelectableLabel(code, codeStyle, GUILayout.Height(codeStyle.CalcHeight(new GUIContent(code), position.width - 24f)));
        }

        private void EnsureStyles()
        {
            if (bodyStyle == null)
            {
                bodyStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true };
            }
            if (codeStyle == null)
            {
                codeStyle = new GUIStyle(EditorStyles.textArea) { font = EditorStyles.miniFont, wordWrap = false };
            }
        }
    }
}
