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

        private SkillTreeController controller;
        private DictionaryStatContext context;
        private ISkillTreeStorage storage;

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

            var canvas = CreateCanvas();

            var viewGo = new GameObject("SkillTreeView", typeof(RectTransform));
            viewGo.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)viewGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            viewGo.AddComponent<SkillTreeView>().Initialize(controller);
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
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
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
