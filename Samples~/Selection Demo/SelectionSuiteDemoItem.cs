using JorisHoef.GenericUIItems;
using JorisHoef.GenericUIItems.CoreState;
using UnityEngine;
using UnityEngine.UI;

namespace JorisHoef.SelectionSuite.Samples.SelectionDemo
{
    public sealed class SelectionSuiteDemoItem : GenericItem<SelectionSuiteDemoData>, ISelectableUIItem
    {
        private static readonly Color NormalColor = new Color(0.18f, 0.19f, 0.20f, 1f);
        private static readonly Color SelectedColor = new Color(0.16f, 0.42f, 0.95f, 1f);

        [SerializeField] private SelectionSuiteDemo owner;
        [SerializeField] private Text label;
        [SerializeField] private Image background;
        [SerializeField] private Button button;
        private bool _isSelected;

        private void Awake()
        {
            EnsureReferences();
            WireButton();
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClicked);
            }
        }

        public void Configure(
            SelectionSuiteDemo demoOwner,
            Text itemLabel,
            Image itemBackground,
            Button itemButton)
        {
            owner = demoOwner;

            if (itemLabel != null)
            {
                label = itemLabel;
            }

            if (itemBackground != null)
            {
                background = itemBackground;
            }

            if (itemButton != null)
            {
                button = itemButton;
            }

            ApplyVisualState();
        }

        public override void SetData(SelectionSuiteDemoData data)
        {
            base.SetData(data);
            EnsureReferences();

            if (label != null)
            {
                label.text = data != null ? data.Label + " (" + data.Id + ")" : string.Empty;
            }
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            ApplyVisualState();
        }

        private void OnClicked()
        {
            if (owner != null && Data != null)
            {
                owner.SelectFromUi(Data.Id);
            }
        }

        private void EnsureReferences()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (label == null)
            {
                label = GetComponentInChildren<Text>();
            }

            if (background == null)
            {
                background = GetComponent<Image>();
            }
        }

        private void WireButton()
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(OnClicked);
            button.onClick.AddListener(OnClicked);
        }

        private void ApplyVisualState()
        {
            if (background != null)
            {
                background.color = _isSelected ? SelectedColor : NormalColor;
            }

            if (label != null)
            {
                label.color = Color.white;
            }
        }
    }
}
