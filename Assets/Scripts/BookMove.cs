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


*/

public class BookMove : MonoBehaviour
{



    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
