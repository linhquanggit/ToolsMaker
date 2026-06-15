using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SkillTree.Samples
{
    public class SkillTreeViewDemo : MonoBehaviour
    {
        [SerializeField] private SkillTreeSO tree;
        [SerializeField] private int startingPoints = 200000;
        [SerializeField] private bool loadSaved = false;
        [SerializeField] private DemoPlayerStats player;

        private SkillTreeController controller;
        private DictionaryStatContext context;
        private ISkillTreeStorage storage;
        private SkillTreeStatsPanel statsPanel;

        private void Start()
        {
            if (tree == null)
            {
                enabled = false;
                return;
            }

            EnsureEventSystem();

            context = new DictionaryStatContext();
            storage = new JsonFileStorage();

            var state = loadSaved && storage.Exists(tree.TreeId)
                ? storage.Load(tree.TreeId)
                : SkillTreeState.CreateFor(tree, startingPoints);

            controller = new SkillTreeController(tree, state, context);
            controller.ReapplyAllEffects();

            ResolvePlayer();
            player.SetContext(context);

            var canvas = CreateCanvas();

            var viewGo = new GameObject("SkillTreeView", typeof(RectTransform));
            viewGo.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)viewGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(0.72f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            viewGo.AddComponent<SkillTreeView>().Initialize(controller);

            var panelGo = new GameObject("StatsPanel", typeof(RectTransform));
            panelGo.transform.SetParent(canvas.transform, false);
            var pr = (RectTransform)panelGo.transform;
            pr.anchorMin = new Vector2(0.72f, 0f);
            pr.anchorMax = new Vector2(1f, 1f);
            pr.offsetMin = Vector2.zero;
            pr.offsetMax = Vector2.zero;
            statsPanel = panelGo.AddComponent<SkillTreeStatsPanel>();
            statsPanel.Initialize(player);

            controller.Events.OnRankChanged += (_, __) => statsPanel.Refresh();
            controller.Events.OnPointsChanged += _ => statsPanel.Refresh();
            controller.Events.OnRespec += statsPanel.Refresh;
        }

        private void ResolvePlayer()
        {
            if (player == null) player = FindObjectOfType<DemoPlayerStats>();
            if (player == null)
            {
                var go = new GameObject("Player");
                player = go.AddComponent<DemoPlayerStats>();
            }
        }

        [Button, EnableIf("@controller != null")]
        private void Respec() => controller?.Respec();

        [Button, EnableIf("@controller != null")]
        private void Save()
        {
            if (controller != null) storage.Save(controller.State);
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject("SkillTreeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }
    }
}
