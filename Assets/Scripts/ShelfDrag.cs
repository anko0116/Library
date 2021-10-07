using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfDrag : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 shelfOrigPos;
    private GameObject bookshelf;

    void Start() {
        bookshelf = GameObject.Find("Bookshelf");
    }

    void OnMouseDown() {
        // Calculate the offset so that
        // dragging shelf doesn't put the shelf on the cursor location
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //offset = transform.position - mousePos;
        //offset = gameObject.transform.position;
        offset = mousePos;
        shelfOrigPos = bookshelf.transform.position;
        //Screen.showCursor = false;
    }

    void OnMouseDrag() {
        MoveWithMouse();
    }

    void OnMouseUp() {
        //Screen.showCursor = true;
    }

    void MoveWithMouse() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 bookPos = bookshelf.transform.position;
        //Vector3 mousePos = bookshelf.transform.position;
        float deltaX = mousePos.x - offset.x;
        float deltaY = mousePos.y - offset.y;
        float newX = ShelfBoundX(shelfOrigPos.x + deltaX);
        float newY = ShelfBoundY(shelfOrigPos.y + deltaY);
        bookshelf.transform.position = new Vector2(newX, newY);
    }

    float ShelfBoundX(float mousePosX) {
        return mousePosX;
        // This bound is not the same as the shelfBounds in MouseLMB script
        float leftBound = -10f;
        float rightBound = 20f;

        if (mousePosX > rightBound) {
            return rightBound;
        }
        else if (mousePosX < leftBound) {
            return leftBound;
        }
        return mousePosX;
    }

    float ShelfBoundY(float mousePosY) {
        return mousePosY;
        // This bound is not the same as the shelfBounds in MouseLMB script
        float topBound = -20f;
        float bottomBound = 10.0f;

        if (mousePosY > topBound) {
            return topBound;
        }
        else if (bottomBound > mousePosY) {
            return bottomBound;
        }
        return mousePosY;
    }
}
