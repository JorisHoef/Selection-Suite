using System.Collections.Generic;
using JorisHoef.Core.State;
using JorisHoef.GenericUIItems;
using JorisHoef.GenericUIItems.CoreState;
using JorisHoef.ObjectSelection;
using JorisHoef.ObjectSelection.CoreState;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JorisHoef.SelectionSuite.Samples.SelectionDemo
{
    public sealed class SelectionSuiteDemo : MonoBehaviour
    {
        private static readonly string[] Keys = { "cube", "sphere", "capsule", "cylinder" };

        private readonly Dictionary<string, GameObject> _worldObjects =
            new Dictionary<string, GameObject>();

        private ObjectSelectionRegistry<string> _objectRegistry;
        private ObjectSelectionService<string> _objectSelection;
        private Repository<string, SelectionSuiteDemoData> _repository;
        private SelectionService<string, SelectionSuiteDemoData> _coreSelection;
        private GenericUIContainer<SelectionSuiteDemoData, string> _uiContainer;
        private RepositoryUIBinding<string, SelectionSuiteDemoData> _repositoryBinding;
        private SelectionUIBinding<string, SelectionSuiteDemoData> _selectionBinding;
        private ObjectSelectionCoreStateBridge<string, SelectionSuiteDemoData> _objectCoreBridge;
        private SelectionSuiteDemoHighlighter _highlighter;
        private ObjectSelectionVisualController<string> _worldVisualController;
        private SelectionSuiteDemoRaycastController _raycastController;

        private RectTransform _itemsParent;
        private GameObject _itemPrefab;
        private Text _currentText;
        private Text _previousText;
        private Text _objectEventText;
        private Text _coreEventText;
        private string _previousKey = "(none)";
        private string _lastObjectEvent = "ObjectSelection: none";
        private string _lastCoreEvent = "CoreState: none";

        private void Awake()
        {
            _objectRegistry = new ObjectSelectionRegistry<string>();
            _objectSelection = new ObjectSelectionService<string>(_objectRegistry);
            _repository = new Repository<string, SelectionSuiteDemoData>();
            _coreSelection = new SelectionService<string, SelectionSuiteDemoData>(_repository);

            EnsureCamera();
            EnsureLight();
            EnsurePrimitives();
            EnsureUi();
            RegisterSharedKeys();
            BindUiToCoreState();
            EnsureHighlighter();

            _objectSelection.SelectionChanged += OnObjectSelectionChanged;
            _coreSelection.SelectionChanged += OnCoreSelectionChanged;

            _objectCoreBridge = new ObjectSelectionCoreStateBridge<string, SelectionSuiteDemoData>(
                _objectSelection,
                _coreSelection);

            EnsureRaycastController();
            UpdateStatusText();
        }

        private void OnDestroy()
        {
            if (_objectSelection != null)
            {
                _objectSelection.SelectionChanged -= OnObjectSelectionChanged;
            }

            if (_coreSelection != null)
            {
                _coreSelection.SelectionChanged -= OnCoreSelectionChanged;
            }

            if (_objectCoreBridge != null)
            {
                _objectCoreBridge.Dispose();
            }

            if (_worldVisualController != null)
            {
                _worldVisualController.Dispose();
            }

            if (_selectionBinding != null)
            {
                _selectionBinding.Dispose();
            }

            if (_repositoryBinding != null)
            {
                _repositoryBinding.Dispose();
            }
        }

        public void SelectFromUi(string key)
        {
            if (_coreSelection != null)
            {
                _coreSelection.TrySelect(key, SelectionChangeMode.Manual);
            }
        }

        private void ClearSelection()
        {
            if (_coreSelection != null)
            {
                _coreSelection.Clear(SelectionChangeMode.Programmatic);
            }
        }

        private void OnObjectSelectionChanged(object sender, JorisHoef.ObjectSelection.SelectionChangedEventArgs<string> args)
        {
            string previous = args.HadPreviousSelection ? args.PreviousKey : "(none)";
            string current = args.HasSelection ? args.CurrentKey : "(none)";
            _lastObjectEvent = "ObjectSelection: " + previous + " -> " + current + " (" + args.Reason + ")";
            UpdateStatusText();
        }

        private void OnCoreSelectionChanged(
            object sender,
            JorisHoef.Core.State.SelectionChangedEventArgs<string, SelectionSuiteDemoData> args)
        {
            _previousKey = args.HadPreviousSelection ? args.PreviousKey : "(none)";

            string current = args.HasSelection ? args.SelectedKey : "(none)";
            _lastCoreEvent = "CoreState: " + _previousKey + " -> " + current + " (" + args.Mode + ")";
            UpdateStatusText();
        }

        private void BindUiToCoreState()
        {
            _uiContainer = new GenericUIContainer<SelectionSuiteDemoData, string>(
                _itemsParent,
                _itemPrefab,
                item => item.Id);

            _repositoryBinding = new RepositoryUIBinding<string, SelectionSuiteDemoData>(
                _repository,
                _uiContainer);

            _selectionBinding = new SelectionUIBinding<string, SelectionSuiteDemoData>(
                _coreSelection,
                _uiContainer);

            _repositoryBinding.Bind();
            _selectionBinding.Bind();
        }

        private void RegisterSharedKeys()
        {
            for (int i = 0; i < Keys.Length; i++)
            {
                string key = Keys[i];

                GameObject target;
                if (!_worldObjects.TryGetValue(key, out target))
                {
                    continue;
                }

                _objectRegistry.Register(new SelectableObject<string>(key, target, target));
                _repository.AddOrUpdate(new SelectionSuiteDemoData(key, ToLabel(key)));
            }
        }

        private void EnsureCamera()
        {
            if (Camera.main != null)
            {
                return;
            }

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.transform.position = new Vector3(0f, 4f, -8f);
            camera.transform.rotation = Quaternion.Euler(25f, 0f, 0f);
        }

        private static void EnsureLight()
        {
            if (FindSceneObject<Light>() != null)
            {
                return;
            }

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private void EnsurePrimitives()
        {
            CreatePrimitive("cube", PrimitiveType.Cube, new Vector3(-3f, 0f, 0f));
            CreatePrimitive("sphere", PrimitiveType.Sphere, new Vector3(-1f, 0f, 0f));
            CreatePrimitive("capsule", PrimitiveType.Capsule, new Vector3(1f, 0f, 0f));
            CreatePrimitive("cylinder", PrimitiveType.Cylinder, new Vector3(3f, 0f, 0f));
        }

        private void CreatePrimitive(string key, PrimitiveType type, Vector3 position)
        {
            if (_worldObjects.ContainsKey(key))
            {
                return;
            }

            GameObject existing = GameObject.Find(key);
            GameObject primitive = existing != null ? existing : GameObject.CreatePrimitive(type);
            primitive.name = key;
            primitive.transform.position = position;
            primitive.transform.rotation = Quaternion.identity;
            _worldObjects.Add(key, primitive);
        }

        private void EnsureUi()
        {
            EnsureEventSystem();

            Canvas canvas = FindSceneObject<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject(
                    "Selection Suite Demo Canvas",
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));

                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280f, 720f);
            }

            RectTransform root = CreatePanel(
                "Selection Suite Panel",
                canvas.transform,
                new Color(0.08f, 0.09f, 0.10f, 0.94f));

            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0f, 1f);
            root.sizeDelta = new Vector2(360f, 430f);
            root.anchoredPosition = new Vector2(16f, -16f);

            VerticalLayoutGroup rootLayout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(14, 14, 12, 12);
            rootLayout.spacing = 8f;
            rootLayout.childForceExpandHeight = false;
            rootLayout.childForceExpandWidth = true;

            Text title = CreateText("Title", root, "JorisHoef Selection Suite", 18, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            AddLayoutElement(title.gameObject, 28f);

            _currentText = CreateText("Current Selection", root, "Current: (none)", 15, TextAnchor.MiddleLeft);
            AddLayoutElement(_currentText.gameObject, 24f);

            _previousText = CreateText("Previous Selection", root, "Previous: (none)", 15, TextAnchor.MiddleLeft);
            AddLayoutElement(_previousText.gameObject, 24f);

            _itemsParent = CreatePanel("Generic UI Items List", root, new Color(0.12f, 0.13f, 0.14f, 1f));
            AddLayoutElement(_itemsParent.gameObject, 190f);

            VerticalLayoutGroup itemsLayout = _itemsParent.gameObject.AddComponent<VerticalLayoutGroup>();
            itemsLayout.padding = new RectOffset(8, 8, 8, 8);
            itemsLayout.spacing = 6f;
            itemsLayout.childForceExpandHeight = false;
            itemsLayout.childForceExpandWidth = true;

            Button clearButton = CreateButton("Clear Selection", root, "Clear Selection");
            clearButton.onClick.AddListener(ClearSelection);
            AddLayoutElement(clearButton.gameObject, 34f);

            _objectEventText = CreateText("Object Selection Event", root, _lastObjectEvent, 12, TextAnchor.MiddleLeft);
            AddLayoutElement(_objectEventText.gameObject, 24f);

            _coreEventText = CreateText("Core State Event", root, _lastCoreEvent, 12, TextAnchor.MiddleLeft);
            AddLayoutElement(_coreEventText.gameObject, 24f);

            GameObject prefabHost = new GameObject("Selection Demo Runtime Prefab Source");
            prefabHost.transform.SetParent(transform, false);
            prefabHost.SetActive(false);
            _itemPrefab = CreateItemPrefab(prefabHost.transform);
        }

        private static void EnsureEventSystem()
        {
            if (FindSceneObject<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private GameObject CreateItemPrefab(Transform prefabHost)
        {
            RectTransform item = CreatePanel(
                "SelectionSuiteDemoItem",
                prefabHost,
                new Color(0.18f, 0.19f, 0.20f, 1f));

            AddLayoutElement(item.gameObject, 38f);

            Button button = item.gameObject.AddComponent<Button>();
            ConfigureButtonColors(button);

            Text label = CreateText("Label", item, "Selection item", 15, TextAnchor.MiddleLeft);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(12f, 0f);
            label.rectTransform.offsetMax = new Vector2(-12f, 0f);

            SelectionSuiteDemoItem sampleItem = item.gameObject.AddComponent<SelectionSuiteDemoItem>();
            sampleItem.Configure(
                this,
                label,
                item.GetComponent<Image>(),
                button);

            return item.gameObject;
        }

        private static Button CreateButton(string name, Transform parentTransform, string text)
        {
            RectTransform buttonTransform = CreatePanel(
                name,
                parentTransform,
                new Color(0.22f, 0.24f, 0.27f, 1f));

            Button button = buttonTransform.gameObject.AddComponent<Button>();
            ConfigureButtonColors(button);

            Text buttonText = CreateText("Text", buttonTransform, text, 14, TextAnchor.MiddleCenter);
            buttonText.rectTransform.anchorMin = Vector2.zero;
            buttonText.rectTransform.anchorMax = Vector2.one;
            buttonText.rectTransform.offsetMin = Vector2.zero;
            buttonText.rectTransform.offsetMax = Vector2.zero;

            return button;
        }

        private static void ConfigureButtonColors(Button button)
        {
            if (button == null)
            {
                return;
            }

            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.28f, 0.31f, 0.35f, 1f);
            colors.pressedColor = new Color(0.16f, 0.42f, 0.95f, 1f);
            colors.selectedColor = new Color(0.20f, 0.34f, 0.52f, 1f);
            button.colors = colors;
        }

        private static RectTransform CreatePanel(string name, Transform parentTransform, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parentTransform, false);

            Image image = panel.GetComponent<Image>();
            image.color = color;

            return panel.GetComponent<RectTransform>();
        }

        private static Text CreateText(string name, Transform parentTransform, string text, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parentTransform, false);

            Text label = textObject.GetComponent<Text>();
            label.text = text;
            label.font = GetBuiltinFont();
            label.fontSize = fontSize;
            label.color = Color.white;
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.rectTransform.sizeDelta = new Vector2(0f, 28f);

            return label;
        }

        private static Font GetBuiltinFont()
        {
            Font font = TryGetBuiltinFont("LegacyRuntime.ttf");
            if (font != null)
            {
                return font;
            }

            font = TryGetBuiltinFont("Arial.ttf");
            if (font != null)
            {
                return font;
            }

            throw new System.InvalidOperationException("No compatible Unity built-in font was found.");
        }

        private static Font TryGetBuiltinFont(string resourceName)
        {
            try
            {
                return Resources.GetBuiltinResource<Font>(resourceName);
            }
            catch (System.ArgumentException)
            {
                return null;
            }
        }

        private static void AddLayoutElement(GameObject gameObject, float preferredHeight)
        {
            LayoutElement element = gameObject.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = gameObject.AddComponent<LayoutElement>();
            }

            element.preferredHeight = preferredHeight;
        }

        private void EnsureHighlighter()
        {
            _highlighter = GetComponent<SelectionSuiteDemoHighlighter>();
            if (_highlighter == null)
            {
                _highlighter = gameObject.AddComponent<SelectionSuiteDemoHighlighter>();
            }

            _worldVisualController = new ObjectSelectionVisualController<string>(_objectSelection, _highlighter);
        }

        private void EnsureRaycastController()
        {
            _raycastController = GetComponent<SelectionSuiteDemoRaycastController>();
            if (_raycastController == null)
            {
                _raycastController = gameObject.AddComponent<SelectionSuiteDemoRaycastController>();
            }

            _raycastController.SelectionCamera = Camera.main;
            _raycastController.ShouldIgnoreInput = IsPointerOverUi;
            _raycastController.Initialize(_objectSelection);
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void UpdateStatusText()
        {
            string current = _coreSelection != null && _coreSelection.HasSelection
                ? _coreSelection.SelectedKey
                : "(none)";

            if (_currentText != null)
            {
                _currentText.text = "Current: " + current;
            }

            if (_previousText != null)
            {
                _previousText.text = "Previous: " + _previousKey;
            }

            if (_objectEventText != null)
            {
                _objectEventText.text = _lastObjectEvent;
            }

            if (_coreEventText != null)
            {
                _coreEventText.text = _lastCoreEvent;
            }
        }

        private static string ToLabel(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return char.ToUpperInvariant(key[0]) + key.Substring(1);
        }

        private static T FindSceneObject<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return FindFirstObjectByType<T>();
#else
            return FindObjectOfType<T>();
#endif
        }
    }
}
