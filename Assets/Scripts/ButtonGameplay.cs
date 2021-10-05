using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGameplay : MonoBehaviour
{
    // Attached to GameplayButton
    GameObject minimap;
    GameObject bookshelf;
    Vector3 mapInitPos;
    Vector3 mapTargetPos;
    Vector3 shelfInitPos;
    Vector3 shelfTargetPos;
    float delta;
    bool mapShown;
    Button gameplayButton;
    bool slide;

    void Start()
    {
        gameplayButton = GetComponent<Button>();
        gameplayButton.onClick.AddListener(TaskOnClick);

        minimap = GameObject.Find("MapCanvas").transform.GetChild(0).gameObject;
        bookshelf = GameObject.Find("Bookshelf");
        mapInitPos = minimap.transform.position;
        mapTargetPos = new Vector3(-3.34f, mapInitPos.y, 0f);
        shelfInitPos = bookshelf.transform.position;
        shelfTargetPos = new Vector3(13f, shelfInitPos.y, 0f);
        delta = 0.01f;
        mapShown = false;
        slide = false;
    }

    void TaskOnClick() {
        slide = true;
    }

    void MoveMapInside(ref Vector3 mapPos, ref Vector3 shelfPos) {
        // TODO: slow down delta
        minimap.transform.position = Vector3.MoveTowards(mapPos, mapTargetPos, delta);
        bookshelf.transform.position = Vector3.MoveTowards(shelfPos, shelfTargetPos, delta);
    }

    void MoveMapOutside(ref Vector3 mapPos, ref Vector3 shelfPos) {
        // TODO: slow down delta
        minimap.transform.position = 
            Vector3.MoveTowards(mapPos, mapInitPos, delta);
        bookshelf.transform.position =
            Vector3.MoveTowards(shelfPos, shelfInitPos, delta);
    }


    void Update()
    {
        if (slide) {
            Vector3 mapPos = minimap.transform.position;
            Vector3 shelfPos = bookshelf.transform.position;
            if (mapShown && mapPos == mapInitPos) {
                slide = false;
                mapShown = false;
            }
            else if (!mapShown && mapPos == mapTargetPos) {
                slide = false;
                mapShown = true;
            }
            else {
                if (mapShown) MoveMapOutside(ref mapPos, ref shelfPos);
                else MoveMapInside(ref mapPos, ref shelfPos);
            }
        }
        
    }
}
