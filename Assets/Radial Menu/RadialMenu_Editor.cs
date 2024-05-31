using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RadialMenu))]
public class NewBehaviourScript : Editor
{

    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();


        RadialMenu rm = (RadialMenu)target;

        GUIContent visualize = new GUIContent("Visualize Arrangement", "Press this to preview what the radial menu will look like ingame.");
        GUIContent reset = new GUIContent("Reset Arrangement", "Press this to reset all elements to a 0 rotation for easy editing.");

        if (!Application.isPlaying)
        {
            if (GUILayout.Button(visualize))
            {

                arrangeElementsInEditor(rm, false);

            }
            if (GUILayout.Button(reset))
            {

                arrangeElementsInEditor(rm, true);

            }

        }

    }

    public void arrangeElementsInEditor(RadialMenu rm, bool reset)
    {

        if (reset)
        {


            for (int i = 0; i < rm.elements.Count; i++)
            {
                if (rm.elements[i] == null)
                {
                    Debug.LogError("Radial Menu: element " + i.ToString() + " in the radial menu " + rm.gameObject.name + " is null!");
                    continue;
                }
                RectTransform elemRt = rm.elements[i].GetComponent<RectTransform>();
                elemRt.rotation = Quaternion.Euler(0, 0, 0);

            }

            return;
        }


        for (int i = 0; i < rm.elements.Count; i++)
        {
            if (rm.elements[i] == null)
            {
                Debug.LogError("Radial Menu: element " + i.ToString() + " in the radial menu " + rm.gameObject.name + " is null!");
                continue;
            }

            RectTransform elemRt = rm.elements[i].GetComponent<RectTransform>();
            RectTransform elemRt2 = rm.elements[i].transform.Find("Button").GetComponent<RectTransform>();
            if (rm.useStickPress)
            {
                if (i == 0)
                {
                    elemRt.rotation = Quaternion.Euler(0, 0, 0 - rm.globalOffset);
                    elemRt2.anchoredPosition = new Vector3(0, 0, 0);
                }
                else
                {
                    var angle = !rm.inverse ? ((360f / (float)(rm.elements.Count - 1)) * (i - 1)) : -((360f / (float)(rm.elements.Count - 1)) * (i - 1));
                    elemRt.rotation = Quaternion.Euler(0, 0, -angle - rm.globalOffset);
                    elemRt2.anchoredPosition = new Vector3(0, 134.3f, 0);
                }
            }
            else
            {
                var angle = !rm.inverse ? ((360f / (float)rm.elements.Count) * i) : -(360f / (float)rm.elements.Count * i);
                elemRt.rotation = Quaternion.Euler(0, 0, -angle - rm.globalOffset);
                elemRt2.anchoredPosition = new Vector3(0, 134.3f, 0);
            }
        }


    }



}
