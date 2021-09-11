using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfDrag : MonoBehaviour
{
    void OnMouseDrag() {
        MoveWithMouse();
    }

    void MoveWithMouse() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //float newObjXPos = GetNearestXCoord(mousePos.x);
        //float newObjYPos = GetNearestYCoord(mousePos.y);
        float newObjXPos = mousePos.x;
        float newObjYPos = mousePos.y;
        Vector2 newObjPos = new Vector2(newObjXPos, newObjYPos);
        transform.position = newObjPos;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
