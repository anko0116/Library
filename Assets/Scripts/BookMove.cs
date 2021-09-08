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

1. Book stays attached to the bookshelf even when bookshelf moves
2. Grab book off the bookshelf and move it around
3. Book changes its orientation depending on which screen it's on
4. Able to drop the book on the table

*/

public class BookMove : MonoBehaviour
{

    GameObject bookshelf; // Neeeded for the position of the bookshelf
    bool onShelf; // Whether the book is on shelf or not
    ShelfMove shelfScr; // Needed to check if shelf grabbed

    float bookRadius;
    int shelfLayer;

    void Start()
    {
        bookshelf = GameObject.Find("Bookshelf");
        onShelf = false;
        shelfScr = bookshelf.GetComponent<ShelfMove>();

        bookRadius = 1.0f;
        shelfLayer = 1;
    }

    void Update()
    {
        if (onShelf) {
            if (shelfScr.grabbed) {
                // Stay with the shelf
                //transform.position = whatever the position should be for the book
            }

        }
        // Off the shelf
        else {
            // if book is in the bookshelf screen
            if (BookInShelfScreen()) {
                //transform.position = FindClosestShelf();
                // TODO: Change orientation of the book

                // Find bookshelf points
                Collider2D[] shelves = Physics2D.OverlapCircleAll(transform.position, bookRadius, shelfLayer);
                if (shelves.Length > 1) {
                    // TODO: put empty gameobjects (with colliders) on the shelves
                    // TODO: Find the closest shelf
                    // TODO: magnet the book on to the shelf


                    //onShelf = true;
                }
            }
            else {
                // TODO: change orientation of the book
                onShelf = false;
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
        // Returns the closest shelf location to the book if book is close enough
    }


}
