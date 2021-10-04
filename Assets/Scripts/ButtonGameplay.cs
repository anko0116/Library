using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGameplay : MonoBehaviour
{
    // Attached to GameplayButton
    GameObject minimap;
    Vector3 initPos;
    Vector3 targetPos;
    float delta;
    bool mapShown;
    Button gameplayButton;
    bool slide;

    void Start()
    {
        gameplayButton = GetComponent<Button>();
        gameplayButton.onClick.AddListener(TaskOnClick);

        minimap = GameObject.Find("MapCanvas").transform.GetChild(0).gameObject;
        initPos = minimap.transform.position;
        targetPos = new Vector3(-3.34f, 0.7f, 0f);
        delta = 0.01f;
        mapShown = false;
        slide = false;
    }

    void TaskOnClick() {
        slide = true;
    }

    void MoveMapInside(ref Vector3 mapPos) {
        // TODO: slow down delta
        minimap.transform.position = Vector3.MoveTowards(mapPos, targetPos, delta);
    }

    void MoveMapOutside(ref Vector3 mapPos) {
        // TODO: slow down delta
        minimap.transform.position = Vector3.MoveTowards(mapPos, initPos, delta);
    }


    void Update()
    {
        if (slide) {
            Vector3 mapPos = minimap.transform.position;
            if (mapShown && mapPos == initPos) {
                slide = false;
                mapShown = false;
            }
            else if (!mapShown && mapPos == targetPos) {
                slide = false;
                mapShown = true;
            }
            else {
                if (mapShown) MoveMapOutside(ref mapPos);
                else MoveMapInside(ref mapPos);
            }
        }
        
    }
}
