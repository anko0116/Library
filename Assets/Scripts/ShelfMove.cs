using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShelfMove : MonoBehaviour
{
    bool grabbed;
    GameObject grabbedObj;

    float shelfMinYCoord = -4.54f;
    float shelfMaxYCoord = 4.54f;

    void Start() {
        grabbed = false;
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

            if (grabbed) {
                grabbed = false;
            }
            else if (hits.Length == 0) {
                Debug.Log("No collider was hit");
            }
            else if (hits.Length > 0) {
                float minYCoord = 100.0f;
                foreach (RaycastHit2D hit in hits) {
                    // Find the closest bottle
                    GameObject currObj = hit.collider.gameObject;
                    float yCoord = currObj.transform.position.y;

                    if (minYCoord > yCoord) {
                        minYCoord = yCoord;
                        grabbedObj = currObj;
                    }
                }
                grabbed = true;
            }
            
        }

        if (grabbed) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //float newObjXPos = GetNearestXCoord(mousePos.x);
            //float newObjYPos = GetNearestYCoord(mousePos.y);
            float newObjXPos = mousePos.x;
            float newObjYPos = mousePos.y;
            CheckShelfBoundary(ref newObjXPos, ref newObjYPos);
            Vector2 newObjPos = new Vector2(newObjXPos, newObjYPos);
            grabbedObj.transform.position = newObjPos;
        }
    }

    void CheckShelfBoundary(ref float newXPos, ref float newYPos) {
        newYPos = Math.Max(newYPos, shelfMinYCoord);
        newYPos = Math.Min(newYPos, shelfMaxYCoord);



    }


    float GetNearestXCoord(float xPos) {
        int nearestXCoord = (int)Math.Round(xPos, 0);
        return nearestXCoord;
    }
    float GetNearestYCoord(float yPos) {
        int nearestYCoord = (int)Math.Round(yPos, 0);
        return nearestYCoord;
    }
}
