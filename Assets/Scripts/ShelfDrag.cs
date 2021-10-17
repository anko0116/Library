using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Attached to Bookshelf 
public class ShelfDrag : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 shelfOrigPos;
    private Vector4 shelfBounds;
    
    private GameObject bookshelf;

    void Start() {
        bookshelf = GameObject.Find("Bookshelf");
        shelfBounds = new Vector4(-1.4f, 10f, -7.5f, 10.3f);
    }

    void OnMouseDown() {
        // Calculate the offset so that
        // dragging shelf doesn't put the shelf on the cursor location
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePos;
        shelfOrigPos = transform.position;
        Cursor.visible = false;
    }

    void OnMouseDrag() {
        MoveWithMouse();
    }

    void OnMouseUp() {
        Cursor.visible = true;
    }

    void MoveWithMouse() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Vector3 mousePos = bookshelf.transform.position;
        float deltaX = mousePos.x - offset.x;
        float deltaY = mousePos.y - offset.y;
        float newX = ShelfBoundX(shelfOrigPos.x + deltaX);
        float newY = ShelfBoundY(shelfOrigPos.y + deltaY);
        bookshelf.transform.position = new Vector2(newX, newY);
    }

    float ShelfBoundX(float mousePosX) {
        if (mousePosX > shelfBounds.y) {
            return shelfBounds.y;
        }
        else if (mousePosX < shelfBounds.x) {
            return shelfBounds.x;
        }
        return mousePosX;
    }

    float ShelfBoundY(float mousePosY) {
        if (mousePosY > shelfBounds.w) {
            return shelfBounds.w;
        }
        else if (mousePosY < shelfBounds.z) {
            return shelfBounds.z;
        }
        return mousePosY;
    }
}
