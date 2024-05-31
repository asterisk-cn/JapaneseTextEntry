using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyUILibrary
{

    // An element that displays RadialMenu
    public class RadialMenu : VisualElement
    {
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            // The selectedIndex property is exposed to UXML.
            UxmlIntAttributeDescription m_SelectedIndexAttribute = new UxmlIntAttributeDescription()
            {
                name = "selectedIndex",
            };

            // Use the Init method to assign the value of the selectedIndex UXML attribute to the C# selectedIndex property.
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                (ve as RadialMenu).selectedIndex = (int)m_SelectedIndexAttribute.GetValueFromBag(bag, cc);
            }
        }

        // Define a factory class to expose this control to UXML.
        public new class UxmlFactory : UxmlFactory<RadialMenu, UxmlTraits> { }

        // These are USS class names for the control overall and the label.
        public static readonly string ussClassName = "radial-menu";
        public static readonly string ussLabelClassName = "radial-menu__label";

        // These objects allow C# code to access custom USS properties.
        static CustomStyleProperty<Color> s_ButtonColor = new CustomStyleProperty<Color>("--button-color");
        static CustomStyleProperty<Color> s_SelectedColor = new CustomStyleProperty<Color>("--selected-color");
        static CustomStyleProperty<Color> s_LineColor = new CustomStyleProperty<Color>("--line-color");

        Color m_ButtonColor = Color.gray;
        Color m_SelectedColor = Color.green;
        Color m_LineColor = Color.black;

        public int numButtons = 5;

        // This is the label that displays the selected index.
        Label m_Label;

        // This is the number that the Label displays as the selected index.
        int m_SelectedIndex;

        // A value between 0 and numButtons that determines the selected button.
        public int selectedIndex
        {
            // The selectedIndex property is exposed in C#.
            get => m_SelectedIndex;
            set
            {
                // Whenever the selectedIndex property changes, MarkDirtyRepaint() is named. This causes a call to the
                // generateVisualContents callback.
                m_SelectedIndex = value;
                m_Label.text = m_SelectedIndex.ToString();
                MarkDirtyRepaint();
            }
        }

        // This default constructor is RadialMenu's only constructor.
        public RadialMenu()
        {
            // Create a Label, add a USS class name, and add it to this visual tree.
            m_Label = new Label();
            m_Label.AddToClassList(ussLabelClassName);
            Add(m_Label);

            // Add the USS class name for the overall control.
            AddToClassList(ussClassName);

            // Register a callback after custom style resolution.
            RegisterCallback<CustomStyleResolvedEvent>(evt => CustomStylesResolved(evt));

            // Register a callback to generate the visual content of the control.
            generateVisualContent += GenerateVisualContent;

            selectedIndex = 0;
        }

        static void CustomStylesResolved(CustomStyleResolvedEvent evt)
        {
            RadialMenu element = (RadialMenu)evt.currentTarget;
            element.UpdateCustomStyles();
        }

        // After the custom colors are resolved, this method uses them to color the meshes and (if necessary) repaint
        // the control.
        void UpdateCustomStyles()
        {
            bool repaint = false;
            if (customStyle.TryGetValue(s_SelectedColor, out m_SelectedColor))
                repaint = true;

            if (customStyle.TryGetValue(s_ButtonColor, out m_ButtonColor))
                repaint = true;

            if (customStyle.TryGetValue(s_LineColor, out m_LineColor))
                repaint = true;

            if (repaint)
                MarkDirtyRepaint();
        }

        void GenerateVisualContent(MeshGenerationContext context)
        {
            float width = contentRect.width;
            float height = contentRect.height;

            float buttonWidth = 40.0f;

            var painter = context.painter2D;
            painter.lineWidth = buttonWidth;
            painter.lineCap = LineCap.Butt;

            // Draw the buttons
            painter.strokeColor = m_ButtonColor;
            painter.BeginPath();
            painter.Arc(new Vector2(width * 0.5f, height * 0.5f), width * 0.5f - buttonWidth * 0.5f, 0.0f, 360.0f);
            painter.Stroke();

            // Draw the selected button
            float selectedAngle = 360.0f * selectedIndex / numButtons - 90.0f - 360.0f / numButtons * 0.5f;
            painter.strokeColor = m_SelectedColor;
            painter.BeginPath();
            painter.Arc(new Vector2(width * 0.5f, height * 0.5f), width * 0.5f - buttonWidth * 0.5f, selectedAngle, selectedAngle + 360.0f / numButtons);
            painter.Stroke();

            // Draw the segments
            float minR = width * 0.5f - buttonWidth;
            float maxR = width * 0.5f;
            painter.lineWidth = 2.0f;
            painter.strokeColor = m_LineColor;
            for (int i = 0; i < numButtons; i++)
            {
                float angle = 360.0f * i / numButtons - 90.0f - 360.0f / numButtons * 0.5f;
                float rad = angle * Mathf.Deg2Rad;
                float x = Mathf.Cos(rad);
                float y = Mathf.Sin(rad);
                painter.BeginPath();
                painter.MoveTo(new Vector2(width * 0.5f + x * minR, height * 0.5f + y * minR));
                painter.LineTo(new Vector2(width * 0.5f + x * maxR, height * 0.5f + y * maxR));
                painter.Stroke();
            }
            painter.BeginPath();
            painter.Arc(new Vector2(width * 0.5f, height * 0.5f), minR, 0.0f, 360.0f);
            painter.Stroke();
            painter.BeginPath();
            painter.Arc(new Vector2(width * 0.5f, height * 0.5f), maxR, 0.0f, 360.0f);
            painter.Stroke();
        }
    }
}
