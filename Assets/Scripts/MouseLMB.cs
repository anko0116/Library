using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
- When a book is certain distance from a shelf spot, it gets attached to it
    - The magnet distance must be small so that game can clearly distinguish between adjacent spots
    - Attach empty gameobject to shelf spots
    - Book bound to the shelf when moving shelf
    - How does the book know which spot is the closest out of all the spots on the shelf screen?
        - Binary search: Left half of the screen or right half? -> left half or right half? -> top half or bottom half?
            Once the shelf is decided, do a linear search on the 12 spots?
            - Data structure to store the shelf points
                - list of list of list...???
                - just a single list but the indices "indicate" the location
        - Hardcode the coordinates for all the spots: only possible when the visuals are FINAL

THREE STATES TO CONSIDER:
    1. ON THE SHELF
    2. HOLDING THE BOOK (table screen or shelf screen)
    3. ON THE TABLE

IMPLEMENTATION ORDER LIST:
    DONE Book changes its orientation depending on which screen it's on while holding
    DONE Able to drop the book on the table
    Done Place the book on the shelf
    DONE Magnet shelf
    DONE move bookshelf by holding and dragging LMB
    DONE Book stays attached to the bookshelf even when bookshelf moves
        Put the book under the gameobject of BookShelf
    DONE Change order of the books when they're on shelf vs otherwise
    - Bookshelf dragging equation = Bookshelf.position + MouseMovement + Time.delta (Something like this)
*/

public class MouseLMB : MonoBehaviour {

    private enum BookState { OnShelf, OnTable, Holding};

    float bookRadius;
    int shelfLayer;

    bool bookGrabbed;
    GameObject grabbedBook;
    GameObject shelfObj;

    Vector4 deskBounds;

    void Start() {
        bookRadius = 0.7f;
        shelfLayer = 1;

        bookGrabbed = false;
        grabbedBook = null;
        shelfObj = GameObject.Find("Bookshelf");

        deskBounds = new Vector4(-8.0f, -1.0f, -1.5f, -0.7f); // left, right, bottom, top
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            // FIXME: may need to use coroutine for linear execution
            bool bookOnShelf = false;
            if (bookGrabbed && ((bookOnShelf = CheckIfShelf()) || CheckIfTable())) {
                // Lower sortingOrder of the book on shelf to 1
                // so that book hides behind the left screen when shelf moves
                if (bookOnShelf) {
                    grabbedBook.GetComponent<SpriteRenderer>().sortingOrder = 1;   
                }
                // Disable control of the book movement
                bookGrabbed = false;
                grabbedBook = null;
                
                // TODO: change the sortingOrder of the book otherwise to 15
            }
            else {
                // Check for colliders that were selected with LMB
                RaycastHit2D[] hits = GetRaycastHits();
                if (hits.Length > 0) {
                    foreach (RaycastHit2D hit in hits) {
                        GameObject currObj = hit.collider.gameObject;

                        if (!bookGrabbed && currObj.tag == "Book") {
                            bookGrabbed = true;
                            grabbedBook = currObj;
                            grabbedBook.GetComponent<SpriteRenderer>().sortingOrder = 15;
                        }
                        // TODO: click on ">>>" to continue dialogue
                        // TODO: click on "GIVE" to submit books
                    }
                }
            }
        }

        if (bookGrabbed) {
            RotateBook();

            // Attach book to closest bookshelf if within distance
            Vector3 closestShelfPos = FindClosestShelf();
            if (closestShelfPos.x != -100) {
                grabbedBook.transform.position = closestShelfPos;
            }
            else {
                MoveWithMouse();
            }
        }
    }

    RaycastHit2D[] GetRaycastHits() {
        // Get mouse position in 3D coordinates
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Convert 3D mouse position to 2D
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        // Find all collider objects that hit on the mouse position
        return Physics2D.RaycastAll(mousePos2D, Vector2.zero);
    }

    void MoveWithMouse() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //float newObjXPos = GetNearestXCoord(mousePos.x);
        //float newObjYPos = GetNearestYCoord(mousePos.y);
        float newObjXPos = mousePos.x;
        float newObjYPos = mousePos.y;
        Vector2 newObjPos = new Vector2(newObjXPos, newObjYPos);
        grabbedBook.transform.position = newObjPos;
    }

    void RotateBook() {
        if (BookInShelfScreen()) {
            grabbedBook.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else {
            grabbedBook.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
    }

    bool BookInShelfScreen() {
        // Returns true if book is inside the shelf screen
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (mousePos.x > 0) {
            return true;
        }
        return false;
    }

    Vector3 FindClosestShelf() {
        /* 
        Used while book is held by the player.
        Returns Vector3 of closest shelf location to the book if book is certain radius from it.
        */

        // Find all the shelves within certain radius
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] shelves = Physics2D.OverlapCircleAll(mousePos, bookRadius, shelfLayer);

        // Find the closest shelf
        Collider2D closestShelf = null;
        float closestDist = float.MaxValue;
        Vector3 bookPos = transform.position;
        foreach (Collider2D coll in shelves) {
            if (coll.gameObject.tag != "Shelf") continue;

            Vector3 shelfPos = coll.transform.position;
            float thisDist = Vector3.Distance(bookPos, shelfPos);
            if (thisDist < closestDist) {
                closestShelf = coll;
                closestDist = thisDist;
            }
        }

        if (closestShelf) {
            return closestShelf.transform.position; 
        }
        return new Vector3(-100, -100, -100);
    }

    bool CheckIfTable() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Checking if on top of desk
        // FIXME: need finer coordinate points
        if (!BookInShelfScreen() && mousePos.x > deskBounds.x &&
            mousePos.x < deskBounds.y && mousePos.y > deskBounds.z
            && mousePos.y < deskBounds.w) {
            return true;
        }
        return false;
    }

    bool CheckIfShelf() {
        Vector3 shelfPos = FindClosestShelf();
        if (shelfPos.x != -100) {
            grabbedBook.transform.position = new Vector3(shelfPos.x, shelfPos.y, 0);
            grabbedBook.transform.parent = shelfObj.transform;
            return true;
        }
        return false;
    }
}
