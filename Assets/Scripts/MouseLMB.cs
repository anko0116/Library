using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    DONE shuffling = inserting books between other books on the shelf (not possible when bookshelf is full)
*/
/*
Table rules
- y-axis locked but x-axis freeflow anywhere
- If you can see the shelf from top to bottom edge of the inner floor, then book can magnet to it
*/

// FIXME: 1. slider animation
//      make minimap go behind in rendering order
// FIXME: 2. snapshot for the minimap (no live update)
// FIXME: 3. move desk back to the left screen
// FIXME: 4. shuffling pushes books permanently
// FIXME: 5. UI buttons
// FIXME: 6. Cassette tape screen (w/ walkman)
// FIXME: 7. Finalize sorting orders

// https://stackoverflow.com/questions/52356828/what-is-the-most-optimal-way-of-communication-between-scripts-in-unity

public class MouseLMB : MonoBehaviour {

    private enum BookState { OnShelf, OnTable, Holding};

    // Constant values
    int maxBookCount;
    float bookRadius;
    int shelfLayer;
    int bookOnShelfLayer;
    int bookOnDeskLayer;
    float stackOffsetVal;
    Vector4 deskBounds;
    Vector4 shelfBounds;
    Vector4 bookBounds;

    // State change variables
    bool bookOnShelf;
    bool shiftedBooks;
    bool bookGrabbed;

    // Used to transition between states
    GameObject shelfSpot;
    GameObject prevShelfSpot;
    GameObject shelfRow;
    GameObject grabbedBook;
    GameObject stackBook;
    List<GameObject> rowSpots;
    List<GameObject> rowBooks;

    void Start() {
        maxBookCount = 12;
        bookRadius = 0.7f;
        shelfLayer = 8;
        bookOnShelfLayer = 8;
        bookOnDeskLayer = 9;
        stackOffsetVal = 0.5f;
        // left-x, right-x, bottom-y, top-y
        deskBounds = new Vector4(0.5f, 6.0f, -1.9f, -0.6f);
        shelfBounds = new Vector4(-5f, 11.4f, -12f, 10f);
        bookBounds = new Vector4(0.5f, 6.3f, -1.9f, 4.75f);

        bookOnShelf = false;
        shiftedBooks = false;
        bookGrabbed = false;

        shelfSpot = null;
        prevShelfSpot = null;
        shelfRow = null;
        grabbedBook = null;
        stackBook = null;
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            // Check if book is droppable
            if (bookGrabbed && (bookOnShelf || CheckIfTable() || stackBook)) {
                if (bookOnShelf) {
                    grabbedBook.GetComponent<SpriteRenderer>().sortingOrder = 2;   
                    grabbedBook.transform.parent = shelfSpot.transform;
                    shelfSpot = null;
                    rowBooks = null;
                    shiftedBooks = false;
                }
                else if (stackBook) {
                    stackBook.GetComponent<SpriteRenderer>().sortingOrder = 14;
                    grabbedBook.transform.parent = stackBook.transform;
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
                            Transform currParent = currObj.transform.parent;
                            bool hasChildBook = false;
                            // If book is in the middle of the stack
                            if (currObj.transform.childCount > 0) {
                                MoveBooksDown(currObj);
                                foreach (Transform child in currObj.transform) {
                                    child.parent = currParent;
                                    hasChildBook = true;
                                }
                            }
                            else if (currParent && currParent.position.x < 0 && !hasChildBook) {
                                currParent.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 15;
                            }

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
            if (BookInShelfScreen(grabbedBook) && (closestShelf = FindClosestShelf())) {
                // Preview book to closest bookshelf if within distance
                bookOnShelf = true;
                shelfSpot = closestShelf;
                shelfRow = shelfSpot.transform.parent.gameObject;
                grabbedBook.layer = bookOnShelfLayer;

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
                    // Bookshelf spot is open!
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
                grabbedBook.layer = bookOnDeskLayer;

                // Book stacking
                stackBook = null;
                if (stackBook = CheckIfBook()) {
                    // TODO:
                    // When book is taken out of the stack, the book is so close
                    // to the stack that it (preview)puts itself back on the stack
                    Vector3 newBookPos = stackBook.transform.position;
                    newBookPos.y += stackOffsetVal;
                    grabbedBook.transform.position = newBookPos;
                } 
                else {
                    MoveWithMouse();
                }
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
        float newObjXPos = Math.Min(mousePos.x, bookBounds.y);
        newObjXPos = Math.Max(newObjXPos, bookBounds.x);
        float newObjYPos = Math.Min(mousePos.y, bookBounds.w);
        newObjYPos = Math.Max(newObjYPos, bookBounds.z);
        Vector2 newObjPos = new Vector2(newObjXPos, newObjYPos);
        grabbedBook.transform.position = newObjPos;
    }

    public void RotateBook(GameObject book = null) {
        /*
        Detects whether the book is in table screen or shelf screen
        and changes the rotation of the book based on its location
        */
        if (book == null) {
            book = grabbedBook;
        }
        Debug.Assert(book != null, "Book object is null.");
        if (BookInShelfScreen(book)) {
            book.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else {
            book.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
    }

    bool BookInShelfScreen(GameObject book) {
        if (!book) return false;
        Vector3 bookPos = book.transform.position;
        if (bookPos.x >= shelfBounds.x && bookPos.x <= shelfBounds.y
            && bookPos.y >= shelfBounds.z && bookPos.y <= shelfBounds.w) {
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
        Collider2D[] shelves = Physics2D.OverlapCircleAll(mousePos, bookRadius, 1 << shelfLayer);

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
        if (mousePos.x > deskBounds.x && mousePos.x < deskBounds.y
            && mousePos.y > deskBounds.z && mousePos.y < deskBounds.w) {
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

    GameObject CheckIfBook() {
        // TODO: refactor this code with FindClosestShelf since they both do same thing
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int bookLayer = 9;
        float bookRadius = 0.5f;
        Collider2D[] books = Physics2D.OverlapCircleAll(mousePos, bookRadius, 1 << bookLayer);

        // Find the closest book
        Collider2D closestBook = null;
        float closestDist = float.MaxValue;
        foreach (Collider2D coll in books) {
            if (coll.gameObject.tag != "Book") continue;
            if (coll.gameObject == grabbedBook) continue;

            Vector3 bookPos = coll.transform.position;
            float thisDist = Vector3.Distance(mousePos, bookPos);
            if (thisDist < closestDist) {
                closestBook = coll;
                closestDist = thisDist;
            }
        }
        
        if (closestBook) {
            return closestBook.gameObject; 
        }
        return null;

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

    void MoveBooksDown(GameObject removingBook) {
        // Moves all the books on top of "removingBook" down by 1 in the stack
        GameObject book = removingBook;
        int cnt = 0;

        // Take all books above removingBook into the stack
        while (book.transform.childCount > 0) {
            foreach (Transform childBook in book.transform) {
                Vector3 downPos = childBook.position;
                childBook.position = 
                    new Vector3(downPos.x, downPos.y - stackOffsetVal + (stackOffsetVal * cnt), downPos.z);
                book = childBook.gameObject;
            }
            ++cnt;
        }
        /*
        // Move books down
        while (books.Count > 0) {
            book = books.Pop();
            book.transform.position = book.transform.parent.position;
        }
        */
    }
}

