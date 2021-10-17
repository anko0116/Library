using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
Table rules
- y-axis locked but x-axis freeflow anywhere
- If you can see the shelf from top to bottom edge of the inner floor, then book can magnet to it
*/

/*
- 30% height of the inner box for the dialogue box (height = 2.79) (DONE)
- 10% height for desk (inner box) (height = 0.93) (DONE)
- Inside rose border, y-range (-4.65, 4.65), x-range (-6.15, 6.15 = 12.25)
*/

/*
BUGS TO FIX:
- When shelves are almost covered by the mask, but the book can still attach to the shelf.
The book will have a spasm!!!
- no interaction until gameplay screen
- shuffling should NOT push books permanently
*/

/*
Phase 4:
- Camera center on character
- Get corresponding dialogue
- Cassette tape
- Limit interactions to just gameplay screen

Interaction:
1. New NPC walks in
2. Dialogue back and forth
(3. Another NPC enters)
3. Gameplay begins (retrieving or organizing)
4. Repeat from #2 until END

- How to organize dialogue file for branching
- Each interaction is a single file? Or all shoved in 1 file
*/

// FIXME: camera shift => library background and character shift? 
//              use Vector3.toward(...)
// FIXME: 2. snapshot for the minimap (no live update)
//        Don't show held book
//         Minimap will show books that are settled on the shelf (permanently locked to the shelf)
// FIXME: 5. UI buttons
// FIXME: 6. Dialogue box (putting in new text and character)
// FIXME: 7. Cassette tape screen (w/ walkman)
// FIXME: 8. Finalize sorting rendering orders
// FIXME: 9. Book and tapes spawning for different days
//      exact locations
// Maybe???: 10. ghost preview (when book can't be placed down. ANYWHERE)
//          this includes EVERY LOCATION YOU CAN'T PUT THE BOOK DOWN.
//          (Should the bookshelf screen also have book grayed out?)

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
    public Vector4 shelfBounds;
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
        bookRadius = 0.05f;
        shelfLayer = 8;
        bookOnShelfLayer = 8;
        bookOnDeskLayer = 9;
        stackOffsetVal = 0.4f;
        // left-x, right-x, bottom-y, top-y
        deskBounds = new Vector4(-1.4f, 1.4f, -1.62f, -1.17f);
        shelfBounds = new Vector4(2.3f, 10f, -7.5f, 10.3f);
        bookBounds = new Vector4(-1.35f, 6f, -1.8f, 4.4f);

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
                    SpriteRenderer bookRend = grabbedBook.GetComponent<SpriteRenderer>();
                    bookRend.sortingOrder = 80;
                    bookRend.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;   
                    grabbedBook.transform.parent = shelfSpot.transform;
                    grabbedBook.layer = bookOnShelfLayer;
                    shelfSpot = null;
                    rowBooks = null;
                    shiftedBooks = false;
                }
                else if (stackBook) {
                    stackBook.GetComponent<SpriteRenderer>().sortingOrder = 80;
                    grabbedBook.transform.parent = stackBook.transform;
                }
                // Disable control of the book movement
                bookGrabbed = false;
                grabbedBook = null;
                Cursor.visible = true;
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
                                currParent.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 80;
                            }

                            bookGrabbed = true;
                            grabbedBook = currObj;
                            SpriteRenderer bookRend = 
                                grabbedBook.GetComponent<SpriteRenderer>();
                            bookRend.sortingOrder = 80;
                            bookRend.maskInteraction = SpriteMaskInteraction.None;
                            grabbedBook.layer = bookOnDeskLayer;
                            grabbedBook.transform.parent = null;
                            Cursor.visible = false;
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
            book.GetComponent<SpriteRenderer>().maskInteraction =
                SpriteMaskInteraction.VisibleOutsideMask;
            
        }
        else {
            book.transform.rotation = Quaternion.Euler(0, 0, 90);
            book.GetComponent<SpriteRenderer>().maskInteraction = 
                SpriteMaskInteraction.None;
        }
    }

    bool BookInShelfScreen(GameObject book) {
        if (!book) return false;
    
        Vector3 bookPos = book.transform.position;
        if (bookPos.x >= shelfBounds.x && bookPos.x <= shelfBounds.y) {
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

