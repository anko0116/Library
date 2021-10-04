using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSlide : MonoBehaviour
{
    GameObject minimap;
    Vector3 targetPos;
    public float delta;
    bool slide;
    void Start()
    {
        minimap = gameObject.transform.GetChild(0).gameObject;
        targetPos = new Vector3(-3.34f, 0.7f, 0f);
        delta = 0.01f;
        slide = true;
    }
    void Update()
    {
        if (slide) {
            MoveMap();
        }
    }

    void MoveMap() {
        // TODO: slow down delta
        Vector3 initPos = minimap.transform.position;
        minimap.transform.position = Vector3.MoveTowards(initPos, targetPos, delta);
    }
}
