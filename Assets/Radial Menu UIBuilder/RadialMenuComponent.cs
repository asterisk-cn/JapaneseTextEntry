using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MyUILibrary;

[RequireComponent(typeof(UIDocument))]
public class RadialMenuComponent : MonoBehaviour
{

    MyUILibrary.RadialMenu m_RadialMenu;
    int numButtons = 5;
    public int selectedIndex = 0;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // m_RadialMenu = new RadialMenu() {
        //     style = {
        //         position = Position.Absolute,
        //         left = 20, top = 20, width = 200, height = 200
        //     }
        // };

        // root.Add(m_RadialMenu);
        m_RadialMenu = root.Q<MyUILibrary.RadialMenu>("RadialMenuLeft");
    }

    void Update()
    {
        // For demo purpose, give the menu property dynamic values.
        selectedIndex = Mathf.Clamp((int)(((Mathf.Sin(Time.time) + 1.0f) / 2.0f) * numButtons), 0, numButtons - 1);
        m_RadialMenu.selectedIndex = selectedIndex;
    }
}
