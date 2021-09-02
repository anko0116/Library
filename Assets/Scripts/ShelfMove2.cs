using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShelfMove2 : MonoBehaviour
{
    bool grabbed;
    GameObject grabbedObj;

    float shelfMinYCoord = -5.54f;
    float shelfMaxYCoord = 5.54f;
    // FIXME: mouse not bounded by the screen
    float shelfMinXCoord = -18.0f;
    float shelfMaxXCoord = 18.0f;

    void Start() {
        grabbed = false;
    }

    void Update() {

        if (Input.GetMouseButtonUp(0)) {
            // Alter grab flag
            grabbed = false;
        }

        else if (grabbed) {
            MoveObject();
        }

        // If left mouse is clicked
        else if (Input.GetMouseButtonDown(0)) {
            // Get mouse position in 3D coordinates
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // Convert 3D mouse position to 2D
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            // Find all collider objects that hit on the mouse position
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

            // If no objects were detected on mouse position
            if (hits.Length == 0) {
                Debug.Log("No collider was hit");
            }
            // If objects were detected
            else if (hits.Length > 0) {
                float minYCoord = 100.0f;
                // Find the closest bottle by comparing objects' y-coord
                foreach (RaycastHit2D hit in hits) {
                    GameObject currObj = hit.collider.gameObject;
                    float yCoord = currObj.transform.position.y;

                    // Keep track of lowest y-coordinate object
                    if (minYCoord > yCoord) {
                        minYCoord = yCoord;
                        grabbedObj = currObj;
                    }
                }
                grabbed = true;
            }
        }
    }

    void CheckShelfBoundary(ref float newXPos, ref float newYPos) {
        // Set y-coordinate boundaries
        newYPos = Math.Max(newYPos, shelfMinYCoord);
        newYPos = Math.Min(newYPos, shelfMaxYCoord);

        // Set x-coordinate boundaries
        newXPos = Math.Max(newXPos, shelfMinXCoord);
        newXPos = Math.Min(newXPos, shelfMaxXCoord);
    }

    void MoveObject() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float newObjXPos = mousePos.x;
        float newObjYPos = mousePos.y;
        CheckShelfBoundary(ref newObjXPos, ref newObjYPos);
        grabbedObj.transform.position = new Vector2(newObjXPos, newObjYPos);;        
    }
}
