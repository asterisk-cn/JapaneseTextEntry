using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class RadialMenu : MonoBehaviour
{

    [HideInInspector]
    public RectTransform rt;

    [Tooltip("If set to true, a pointer with a graphic of your choosing will aim in the direction of your mouse. You will need to specify the container for the selection follower.")]
    public bool useSelectionFollower = true;

    [Tooltip("If using the selection follower, this must point to the rect transform of the selection follower's container.")]
    public RectTransform selectionFollowerContainer;

    [Tooltip("This is the text object that will display the labels of the radial elements when they are being hovered over. If you don't want a label, leave this blank.")]
    public Text textLabel;

    [Tooltip("This is the list of radial menu elements. This is order-dependent. The first element in the list will be the first element created, and so on.")]
    public List<RadialMenuElement> elements = new List<RadialMenuElement>();


    // TODO: set from TextEntryInputs?
    public float globalOffset = 0f;

    // TODO: set from TextEntryInputs?
    public bool useStickPress = true;
    public bool inverse = false;


    [HideInInspector]
    public int index = 0; //The current index of the element we're pointing at.

    private int elementCount;

    private float angleOffset; //The base offset. For example, if there are 4 elements, then our offset is 360/4 = 90

    private int previousActiveIndex = 0; //Used to determine which buttons to unhighlight in lazy selection.

    private PointerEventData pointer;

    void Awake()
    {
        pointer = new PointerEventData(EventSystem.current);

        rt = GetComponent<RectTransform>();

        if (rt == null)
            Debug.LogError("Radial Menu: Rect Transform for radial menu " + gameObject.name + " could not be found. Please ensure this is an object parented to a canvas.");

        if (useSelectionFollower && selectionFollowerContainer == null)
            Debug.LogError("Radial Menu: Selection follower container is unassigned on " + gameObject.name + ", which has the selection follower enabled.");

        elementCount = elements.Count;

        SetRadialMenu();
    }

    // Update is called once per frame
    void Update()
    {

        //Updates the selection follower if we're using one.
        // if (useSelectionFollower && selectionFollowerContainer != null)
        // {
        //     if (!useGamepad || joystickMoved)
        //         selectionFollowerContainer.rotation = Quaternion.Euler(0, 0, rawAngle + 270);
        // }

    }

    void SetRadialMenu()
    {
        for (int i = 0; i < elementCount; i++)
        {
            if (elements[i] == null)
            {
                Debug.LogError("Radial Menu: element " + i.ToString() + " in the radial menu " + gameObject.name + " is null!");
                continue;
            }
            elements[i].parentRM = this;
            elements[i].assignedIndex = i;
        }
    }

    public void UpdateRadialMenu()
    {
        if (useStickPress)
            angleOffset = (360f / (float)(elementCount - 1));
        else
            angleOffset = (360f / (float)(elementCount));

        //Loop through and set up the elements.
        for (int i = 0; i < elements.Count; i++)
        {
            if (useStickPress)
            {
                if (i == 0)
                {
                    elements[i].setAllAngles(0 + globalOffset);
                    elements[i].SetPositionYOffset(0);
                }
                else
                {
                    var angle = !inverse ? (angleOffset * (i - 1)) : -(angleOffset * (i - 1));
                    elements[i].setAllAngles(angle + globalOffset);
                    elements[i].SetPositionYOffset(134.3f);
                }
            }
            else
            {
                var angle = !inverse ? (angleOffset * i) : -(angleOffset * i);
                elements[i].setAllAngles(angle + globalOffset);
                elements[i].SetPositionYOffset(134.3f);
            }

            elements[i].UpdateTransform();
        }
    }


    //Selects the button with the specified index.
    public void selectButton(int i)
    {

        if (elements[i].active == false)
        {

            elements[i].highlightThisElement(pointer); //Select this one

            if (previousActiveIndex != i)
                elements[previousActiveIndex].unHighlightThisElement(pointer); //Deselect the last one.


        }

        previousActiveIndex = i;

    }

    public void ClearSelect()
    {
        elements[previousActiveIndex].unHighlightThisElement(pointer);
    }

    public void setLabels(string[] labels)
    {
        for (int i = 0; i < elementCount; i++)
        {
            elements[i].setLabelText(labels[i]);
        }

    }

    //Keeps angles between 0 and 360.
    private float normalizeAngle(float angle)
    {

        angle = angle % 360f;

        if (angle < 0)
            angle += 360;

        return angle;

    }


}
