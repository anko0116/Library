using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGameplay : MonoBehaviour
{
    // Attached to GameplayButton
    GameObject minimap;
    GameObject bookshelf;
    GameObject blackBorder;

    Vector3 mapInitPos;
    Vector3 mapTargetPos;
    Vector3 shelfInitPos;
    Vector3 shelfTargetPos;
    Vector3 npcInitPos;
    Vector3 npcTargetPos;
    List<List<GameObject>> npcSpots;

    float delta;
    bool mapShown;
    Button gameplayButton;
    public bool slide;
    public int spotIdx; 

    void Start() {
        gameplayButton = GetComponent<Button>();
        gameplayButton.onClick.AddListener(TaskOnClick);

        minimap = GameObject.Find("MapCanvas").transform.GetChild(0).gameObject;
        bookshelf = GameObject.Find("Bookshelf");

        npcSpots = new List<List<GameObject>>();
        npcSpots.Add(new List<GameObject>());
        npcSpots[0].Add(GameObject.Find("NPCSpot"));

        mapInitPos = minimap.transform.position;
        mapTargetPos = new Vector3(-1.6f, mapInitPos.y, 0f);
        shelfInitPos = bookshelf.transform.position;
        shelfTargetPos = new Vector3(10.64f, shelfInitPos.y, 0f);
        npcInitPos = npcSpots[0][0].transform.position;
        npcTargetPos = new Vector3(npcInitPos.x - 4.5f, npcInitPos.y, 0f);

        delta = 0.05f;
        mapShown = false;
        slide = false;
        spotIdx = 0;
    }

    void TaskOnClick() {
        slide = true;
    }

    void MoveMapInside(ref Vector3 mapPos, ref Vector3 shelfPos, ref Vector3 npcPos) {
        // TODO: slow down delta
        minimap.transform.position = Vector3.MoveTowards(mapPos, mapTargetPos, delta*2);
        bookshelf.transform.position = Vector3.MoveTowards(shelfPos, shelfTargetPos, delta);
        npcSpots[0][0].transform.position = Vector3.MoveTowards(npcPos, npcTargetPos, delta);
    }

    void MoveMapOutside(ref Vector3 mapPos, ref Vector3 shelfPos, ref Vector3 npcPos) {
        // TODO: slow down delta
        minimap.transform.position = 
            Vector3.MoveTowards(mapPos, mapInitPos, delta*2);
        bookshelf.transform.position =
            Vector3.MoveTowards(shelfPos, shelfInitPos, delta);
        npcSpots[0][0].transform.position = Vector3.MoveTowards(npcPos, npcInitPos, delta);
    }

    void Update()
    {
        Vector3 mapPos = minimap.transform.position;
        Vector3 shelfPos = bookshelf.transform.position;
        Vector3 npcPos = npcSpots[0][0].transform.position;
        if (slide) {
            if (mapShown && mapPos == mapInitPos) {
                slide = false;
                mapShown = false;
            }
            else if (!mapShown && mapPos == mapTargetPos) {
                slide = false;
                mapShown = true;
            }
            else {
                if (mapShown) MoveMapOutside(ref mapPos, ref shelfPos, ref npcPos);
                else MoveMapInside(ref mapPos, ref shelfPos, ref npcPos);
            }
        }
        
    }
}
