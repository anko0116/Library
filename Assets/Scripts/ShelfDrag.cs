using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfDrag : MonoBehaviour
{
    private Vector3 offset;

    void OnMouseDown() {
        // Calculate the offset so that
        // dragging shelf doesn't put the shelf on the cursor location
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - mousePos;
    }

    void OnMouseDrag() {
        MoveWithMouse();
    }

    void MoveWithMouse() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float newObjXPos = ShelfBoundX(mousePos.x + offset.x);
        float newObjYPos = ShelfBoundY(mousePos.y + offset.y);
        transform.position = new Vector2(newObjXPos, newObjYPos);
    }

    float ShelfBoundX(float mousePosX) {
        float leftBound = -9.0f;
        float rightBound = 18.0f;

        if (mousePosX > rightBound) {
            return rightBound;
        }
        else if (mousePosX < leftBound) {
            return leftBound;
        }
        return mousePosX;
    }

    float ShelfBoundY(float mousePosY) {
        float topBound = 5.54f;
        float bottomBound = -5.54f;

        if (mousePosY > topBound) {
            return topBound;
        }
        else if (bottomBound > mousePosY) {
            return bottomBound;
        }
        return mousePosY;
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
