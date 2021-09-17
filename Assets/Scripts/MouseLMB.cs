using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
--- Sorting order for all objects ---
Book 15 or 2
Character 10
Desk 10
Library background 5
Bookshelf 1
Bookshelf background 0

*/

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
    DONE Change sortingOrder of the books when they're on shelf vs otherwise
    DONE Bookshelf dragging equation = Bookshelf.position + MouseMovement + Time.delta (Something like this)
    DONE prevent placing books on top of each other (not talking about stacking)
    DONE Move all the ShelfSpot objects into correct positions
    DONE bug: when grabbing book off the shelf, the book magnets towards a farther away shelf
    DONE Skip magnet when book is under it
    DONE make space for 12 rowSpots
    DONE? shuffling = inserting books between other books on the shelf (not possible when bookshelf is full)

    - don't let books stack on top of each on the table
*/


// https://stackoverflow.com/questions/52356828/what-is-the-most-optimal-way-of-communication-between-scripts-in-unity

public class MouseLMB : MonoBehaviour {

    private enum BookState { OnShelf, OnTable, Holding};

    int maxBookCount;
    float bookRadius;
    int shelfLayer;
    bool bookOnShelf;
    GameObject shelfSpot;
    GameObject prevShelfSpot;
    GameObject shelfRow;
    List<GameObject> rowSpots;
    List<GameObject> rowBooks;

    bool bookGrabbed;
    GameObject grabbedBook;
    //GameObject shelf;

    Vector4 deskBounds;

    bool shiftedBooks;
    bool shiftedLeft;

    void Start() {
        maxBookCount = 12;
        bookRadius = 0.7f;
        shelfLayer = 1;
        bookOnShelf = false;
        shelfSpot = null;
        prevShelfSpot = null;
        shelfRow = null;

        bookGrabbed = false;
        grabbedBook = null;
        //shelf = GameObject.Find("Bookshelf");

        deskBounds = new Vector4(-8.0f, -1.0f, -1.5f, -0.7f); // left, right, bottom, top

        shiftedBooks = false;
        shiftedLeft = true;
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            // Check if book is droppable
            if (bookGrabbed && (bookOnShelf || CheckIfTable())) {
                if (bookOnShelf) {
                    grabbedBook.GetComponent<SpriteRenderer>().sortingOrder = 2;   
                    grabbedBook.transform.parent = shelfSpot.transform;
                    shelfSpot = null;
                    rowBooks = null;
                    shiftedBooks = false;
                }
                // Disable control of the book movement
                bookGrabbed = false;
                grabbedBook = null;
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
                            grabbedBook.transform.parent = null;
                        }
                        // TODO: click on ">>>" to continue dialogue
                        // TODO: click on "GIVE" to submit books
                    }
                }
            }
        }

        // All book-holding interactions
        if (bookGrabbed) {
            RotateBook();
            GameObject closestShelf = null;
            if (BookInShelfScreen() && (closestShelf = FindClosestShelf())) {
                // Preview book to closest bookshelf if within distance
                bookOnShelf = true;
                shelfSpot = closestShelf;
                shelfRow = shelfSpot.transform.parent.gameObject;

                bool spotHasBook = closestShelf.transform.childCount != 0;
                if (spotHasBook) {
                    if (shiftedBooks && !GameObject.ReferenceEquals(shelfSpot, prevShelfSpot)) {
                        ReturnBooksToSpot();
                        rowBooks = null;
                        shiftedBooks = false;
                    }

                    // Data for shifting books
                    int spotIndex = GetShelfSpotIndex(closestShelf.name);
                    rowSpots = GetShelfSpots();
                    rowBooks = GetShelfBooks(shelfRow);
                    shiftedBooks = ShiftBooks(spotIndex);

                    if (shiftedBooks) {
                        grabbedBook.transform.position = closestShelf.transform.position;
                        // FIXME: this could cause bugs
                        prevShelfSpot = shelfSpot;
                        shelfSpot = null;
                        shiftedBooks = true;
                    }
                    else {
                        MoveWithMouse();
                        bookOnShelf = false;
                    }
                }
                else {
                    grabbedBook.transform.position = closestShelf.transform.position;
                }
            }
            else {
                if (shiftedBooks) {
                    ReturnBooksToSpot();
                    rowBooks = null;
                    shiftedBooks = false;
                }
                bookOnShelf = false;
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
        //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (grabbedBook.transform.position.x > 0) {
            return true;
        }
        return false;
    }

    GameObject FindClosestShelf() {
        /* 
        - Used while book is held by the player
        - grabbedBook is not empty when this method is called
        - Returns GameObject of the closest shelf to the book if book is certain radius from it
        */

        // Find all the shelves within certain radius
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] shelves = Physics2D.OverlapCircleAll(mousePos, bookRadius, shelfLayer);

        // Find the closest shelf
        Collider2D closestShelf = null;
        float closestDist = float.MaxValue;
        foreach (Collider2D coll in shelves) {
            if (coll.gameObject.tag != "Shelf") continue;
            //if (coll.gameObject.transform.childCount > 0) continue;

            Vector3 shelfPos = coll.transform.position;
            float thisDist = Vector3.Distance(mousePos, shelfPos);
            if (thisDist < closestDist) {
                closestShelf = coll;
                closestDist = thisDist;
            }
        }

        if (closestShelf) {
            return closestShelf.gameObject; 
        }
        return null;
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
        // Used when placing the book on the shelf
        if (bookOnShelf && shelfSpot) {
            //Vector3 shelfPos = shelf.transform.position;
            //grabbedBook.transform.position = new Vector3(shelfPos.x, shelfPos.y, 0);
            return true;
        }
        return false;
    }

    bool ShiftBooks(int spotIndex) {
        // Moves books on the shelf left or right to make space for new book
        // if the shelf is not full.
        // Modifies shuffleLeft bool to indicate which way the books got moved.

        // FIXME: Code refactor please...
        bool checkLeft = true;
        if (!ShelfFullDirection(checkLeft, spotIndex)) {
            for (int i = spotIndex-1; i > -1; --i) {
                bool foundEmpty = false;
                if (rowSpots[i].transform.childCount == 0) {
                    foundEmpty = true;
                }
                GameObject rightBook = GetShelfSpotBook(rowSpots[i+1]);
                if (!rightBook) {
                    return true;
                }
                rightBook.transform.parent = rowSpots[i].transform;
                rightBook.transform.position = rowSpots[i].transform.position; 
                if (foundEmpty) {
                    return true;
                }
            }
            return true;
        }
        
        
        if (!ShelfFullDirection(!checkLeft, spotIndex)) {
            for (int i = spotIndex+1; i < maxBookCount; ++i) {
                bool foundEmpty = false;
                if (rowSpots[i].transform.childCount == 0) {
                    foundEmpty = true;
                }
                GameObject leftBook = GetShelfSpotBook(rowSpots[i-1]);
                if (!leftBook) {
                    return true;
                }
                leftBook.transform.parent = rowSpots[i].transform;
                leftBook.transform.position = rowSpots[i].transform.position;
                if (foundEmpty) {
                    return true;
                }
            }           
            return true;
        }

        // Hit here if shelf is full.
        return false;
    }

    bool ShelfFullDirection(bool checkLeft, int spotIndex) {
        for (int i = 0; i < maxBookCount; ++i) {
            int spotChildCnt = rowSpots[i].transform.childCount;
            if (checkLeft && i < spotIndex && spotChildCnt == 0) {
                return false;
            }
            else if (!checkLeft && i > spotIndex && spotChildCnt == 0) {
                return false;
            }
        }
        return true;
    }

    GameObject GetShelfSpotBook(GameObject shelfSpot) {
        foreach (Transform childBook in shelfSpot.transform) {
            return childBook.gameObject;
        }
        return null;
    }

    List<GameObject> GetShelfSpots() {
        List<GameObject> shelfBooks = new List<GameObject>(new GameObject[maxBookCount]);
        int i = 0;
        foreach (Transform spot in shelfRow.transform) {
            shelfBooks[i++] = spot.gameObject;
        }
        return shelfBooks;
    }

    List<GameObject> GetShelfBooks(GameObject shelfRow) {
        List<GameObject> books = new List<GameObject>();
        foreach (Transform spot in shelfRow.transform) {
            if (spot.transform.childCount == 0) {
                books.Add(null);
            }
            else {
                foreach (Transform book in spot) {
                    books.Add(book.gameObject);
                }
            }
        }
        return books;
    }

    void ReturnBooksToSpot() {
        for (int i = 0; i < maxBookCount; ++i) {
            if (rowBooks[i]) {
                rowBooks[i].transform.parent = rowSpots[i].transform;
                rowBooks[i].transform.position = rowSpots[i].transform.position;
            }
        }
    }

    int GetShelfSpotIndex(string objName) {
        char char1 = objName[objName.Length-2];
        char char2 = objName[objName.Length-1];
        if (char1 - '0' >= 0 && char1 - '0' <= 9) {
            return (char1 - '0') * 10 + char2 - '0';
        }
        return char2 - '0';
    }
}
