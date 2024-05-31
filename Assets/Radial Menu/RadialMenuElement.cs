using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class RadialMenuElement : MonoBehaviour
{

    [HideInInspector]
    public RectTransform rt;
    [HideInInspector]
    public RectTransform rt2;
    [HideInInspector]
    public RadialMenu parentRM;

    [Tooltip("Each radial element needs a button. This is generally a child one level below this primary radial element game object.")]
    public Button button;

    public Text labelText;

    [Tooltip("This is the text label that will appear in the center of the radial menu when this option is moused over. Best to keep it short.")]
    public string label;

    public Color normalColor;
    public Color highlightedColor;

    private Image img;

    [HideInInspector]
    public float _angleOffset;
    [HideInInspector]
    public float _positionYOffset;

    [HideInInspector]
    public bool active = false;

    [HideInInspector]
    public int assignedIndex = 0;
    // Use this for initialization

    private CanvasGroup cg;
    void Awake()
    {

        rt = gameObject.GetComponent<RectTransform>();
        rt2 = gameObject.transform.Find("Button").GetComponent<RectTransform>();

        _positionYOffset = rt2.anchoredPosition.y;

        if (gameObject.GetComponent<CanvasGroup>() == null)
            cg = gameObject.AddComponent<CanvasGroup>();
        else
            cg = gameObject.GetComponent<CanvasGroup>();

        img = button.GetComponent<Image>();
        img.color = normalColor;

        if (rt == null)
            Debug.LogError("Radial Menu: Rect Transform for radial element " + gameObject.name + " could not be found. Please ensure this is an object parented to a canvas.");

        if (button == null)
            Debug.LogError("Radial Menu: No button attached to " + gameObject.name + "!");

        if (labelText == null)
            Debug.LogError("Radial Menu: No label text attached to " + gameObject.name + "!");

    }

    void Start()
    {

        //We don't want our normal mouse-over effects interfering, so we turn raycasts off.
        cg.blocksRaycasts = false;
    }

    //Used by the parent radial menu to set up all the approprate angles. Affects master Z rotation and the active angles for lazy selection.
    public void setAllAngles(float offset)
    {

        _angleOffset = offset;

    }

    public void SetPositionYOffset(float yOffset)
    {

        _positionYOffset = yOffset;

    }

    public void UpdateTransform(float angleOffset, float positionYOffset)
    {

        _angleOffset = angleOffset;
        _positionYOffset = positionYOffset;

        rt2.anchoredPosition = new Vector3(0, positionYOffset, 0);
        rt.rotation = Quaternion.Euler(0, 0, -angleOffset);

    }

    public void UpdateTransform()
    {

        rt2.anchoredPosition = new Vector3(0, _positionYOffset, 0);
        rt.rotation = Quaternion.Euler(0, 0, -_angleOffset);

    }

    //Highlights this button. Unity's default button wasn't really meant to be controlled through code so event handlers are necessary here.
    //I would highly recommend not messing with this stuff unless you know what you're doing, if one event handler is wrong then the whole thing can break.
    public void highlightThisElement(PointerEventData p)
    {

        // ExecuteEvents.Execute(button.gameObject, p, ExecuteEvents.selectHandler);
        img.color = highlightedColor;
        active = true;
        setParentMenuLable(label);

    }

    //Sets the label of the parent menu. Is set to public so you can call this elsewhere if you need to show a special label for something.
    public void setParentMenuLable(string l)
    {

        if (parentRM.textLabel != null)
            parentRM.textLabel.text = l;


    }


    //Unhighlights the button, and if lazy selection is off, will reset the menu's label.
    public void unHighlightThisElement(PointerEventData p)
    {

        // ExecuteEvents.Execute(button.gameObject, p, ExecuteEvents.deselectHandler);
        img.color = normalColor;
        active = false;

        setParentMenuLable(" ");


    }

    public void setLabelText(string l)
    {

        labelText.text = l;

    }

    //Just a quick little test you can run to ensure things are working properly.
    public void clickMeTest()
    {

        Debug.Log(assignedIndex);


    }
}

