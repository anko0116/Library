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
    1. Book stays attached to the bookshelf even when bookshelf moves
    2. Grab book off the bookshelf and move it around
    3. Book changes its orientation depending on which screen it's on
    4. Able to drop the book on the table
*/

public class BookMove : MonoBehaviour
{
    private enum BookState { OnShelf, OnTable, Holding};

    GameObject bookshelf; // Neeeded for the position of the bookshelf
    ShelfMove shelfScr; // Needed to check if shelf grabbed
    BookState bookState;

    float bookRadius;
    int shelfLayer;

    void Start()
    {
        bookshelf = GameObject.Find("Bookshelf");
        shelfScr = bookshelf.GetComponent<ShelfMove>();
        bookState = BookState.OnShelf;

        bookRadius = 1.0f;
        shelfLayer = 1;
    }

    void Update() {
        // TODO: what to do when an input comes in?
        /* FIXME: 
            change this script to be attached to the main camera
            and takes the mouse input. Don't attach this script to 
            all the books (bad efficiency).

            Consider just the book that the player is interacting with
        */

        if (bookState == BookState.OnShelf) {
            if (shelfScr.grabbed) {
                // Move with the shelf
                //transform.position = whatever the position should be for the book
            }
        }
        else if (bookState == BookState.OnTable) {

        }
        else {
            if (BookInShelfScreen()) {
                // TODO: Change orientation of the book

                // Attach book to closest bookshelf if within distance
                Vector3 closestShelfPos = FindClosestShelf();
                if (closetShelfPos.x != -100) {
                    transform.position = closetShelfPos;
                }
            }
            else {
                // TODO: change orientation of the book
                
            }
        }

    }

    bool BookInShelfScreen() {
        // Returns true if book is inside the shelf screen
        Vector3 bookPos = transform.position;
        if (bookPos.x > 0) {
            return true;
        }
        return false;
    }

    void FindClosestShelf() {
        /* 
        Used while book is held by the player.
        Returns the closest shelf location to the book if book is certain radius from it.
        */

        // Find all the shelves within certain radius
        Collider2D[] shelves = Physics2D.OverlapCircleAll(transform.position, bookRadius, shelfLayer);

        // Find the closest shelf
        GameObject closestShelf = null;
        float closestDist = Single.MaxValue;
        Vector3 bookPos = transform.position;
        foreach (Collider2D coll in shelves) {
            if (coll.GameObject.tag != "bookshelf")  continue;

            Vector3 shelfPos = coll.transform.position;
            float thisDist = Vector3.Distance(bookPos, shelfPos);
            if (thisDist < closestDist) {
                closetShelf = coll;
                closestDist = thisDist;
            }
        }

        if (closetShelf) {
            bookState = BookState.OnShelf;
            return closestShelf.transform.position; 
        }
        return new Vector3(-100, -100, -100);
    }
}
