using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Parkour
{
    /// <summary>
    /// Ajudante que monta a interface (Canvas, textos, botões, sliders) por CÓDIGO.
    /// Assim as telas não dependem de prefabs nem de configuração manual no Inspector.
    /// </summary>
    public static class UiBuilder
    {
        public static readonly Color Pink = new Color(0.90f, 0.05f, 0.45f);
        public static readonly Color Dark = new Color(0.08f, 0.05f, 0.15f);

        static Font font;

        /// <summary>Fonte padrão embutida na Unity.</summary>
        public static Font DefaultFont
        {
            get
            {
                if (font == null)
                {
                    font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 16);
                }
                return font;
            }
        }

        /// <summary>Garante que existe um EventSystem (necessário para clicar nos botões).</summary>
        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }

        public static Canvas CreateCanvas(string name, int sortingOrder = 0)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();
            return canvas;
        }

        static Image NewImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        /// <summary>Painel que cobre a tela inteira.</summary>
        public static Image FullPanel(Transform parent, Color color)
        {
            var img = NewImage("Painel", parent, color);
            SetAnchors(img.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return img;
        }

        /// <summary>
        /// Texto. "pos" é relativo ao ponto de ancoragem (centro da tela por padrão).
        /// Passe anchor (0,1) para o canto superior esquerdo, (1,1) para o direito etc.
        /// </summary>
        public static Text Label(Transform parent, string text, int fontSize, TextAnchor align,
                                 Vector2 pos, Vector2 size, Color color, Vector2? anchor = null)
        {
            var go = new GameObject("Texto", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var t = go.AddComponent<Text>();
            t.font = DefaultFont;
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = align;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;

            Vector2 a = anchor ?? new Vector2(0.5f, 0.5f);
            RectTransform rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = a;
            rt.pivot = a;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return t;
        }

        /// <summary>
        /// Botão. Por padrão fica relativo ao centro da tela; passe "anchor" (ex.: (1,1) = canto superior direito)
        /// para botões fixos num canto. A navegação por teclado é desligada e o botão é "desselecionado"
        /// depois do clique, senão apertar Espaço (pular) clicaria no botão de novo.
        /// </summary>
        public static Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size, UnityAction onClick,
                                        int fontSize = 42, Vector2? anchor = null)
        {
            var img = NewImage(label + "_Botao", parent, Pink);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;

            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 0.40f, 0.70f);
            colors.pressedColor = new Color(0.60f, 0.02f, 0.30f);
            btn.colors = colors;

            Vector2 a = anchor ?? new Vector2(0.5f, 0.5f);
            RectTransform rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = a;
            rt.pivot = a;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Label(img.transform, label, fontSize, TextAnchor.MiddleCenter, Vector2.zero, size, Color.white);
            btn.onClick.AddListener(onClick);
            btn.onClick.AddListener(Deselect);
            return btn;
        }

        static void Deselect()
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        public static Slider MakeSlider(Transform parent, Vector2 pos, Vector2 size,
                                        float min, float max, float value, UnityAction<float> onChanged)
        {
            var go = new GameObject("Slider", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            // trilho
            var bg = NewImage("Fundo", go.transform, new Color(0.22f, 0.20f, 0.30f));
            SetAnchors(bg.rectTransform, new Vector2(0f, 0.3f), new Vector2(1f, 0.7f), Vector2.zero, Vector2.zero);

            // parte preenchida
            var fillArea = new GameObject("Area do Preenchimento", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            SetAnchors((RectTransform)fillArea.transform, new Vector2(0f, 0.3f), new Vector2(1f, 0.7f),
                       new Vector2(5f, 0f), new Vector2(-15f, 0f));
            var fill = NewImage("Preenchimento", fillArea.transform, Pink);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.sizeDelta = new Vector2(10f, 0f);

            // bolinha
            var handleArea = new GameObject("Area da Bolinha", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            SetAnchors((RectTransform)handleArea.transform, Vector2.zero, Vector2.one,
                       new Vector2(10f, 0f), new Vector2(-10f, 0f));
            var handle = NewImage("Bolinha", handleArea.transform, Color.white);
            handle.rectTransform.anchorMin = new Vector2(0f, 0f);
            handle.rectTransform.anchorMax = new Vector2(0f, 1f);
            handle.rectTransform.sizeDelta = new Vector2(30f, 0f);

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            var sn = slider.navigation;
            sn.mode = Navigation.Mode.None;     // A/D e setas não mexem no slider durante o jogo
            slider.navigation = sn;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.onValueChanged.AddListener(onChanged);
            return slider;
        }
    }
}
