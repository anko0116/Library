using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSlide : MonoBehaviour
{
    GameObject minimap;
    Vector3 targetPos;
    public float delta;
    bool slide;
    bool mapShown;
    void Start()
    {
        minimap = gameObject.transform.GetChild(0).gameObject;
        targetPos = new Vector3(-3.34f, 0.7f, 0f);
        delta = 0.05f;
        slide = true;
        mapShown = false;
    }
    void Update()
    {
        if (slide) {
            MoveMap();
        }
    }

    void MoveMap() {
        Vector3 initPos = minimap.transform.position;
        minimap.transform.position = Vector3.MoveTowards(initPos, targetPos, delta);
    }
}
