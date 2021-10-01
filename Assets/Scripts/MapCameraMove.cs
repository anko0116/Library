using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCameraMove : MonoBehaviour
{
    GameObject shelfCamera;
    GameObject bookshelf;
    void Start()
    {
        shelfCamera = GameObject.Find("ShelfMapCamera");    
        bookshelf = GameObject.Find("Bookshelf");
    }

    void Update()
    {
        // FIXME: need to get the accurate coordinates for the bookshelf!
        Vector3 shelfPos = bookshelf.transform.position; 
        Vector3 camPos = shelfCamera.transform.position;
        shelfCamera.transform.position = new Vector3(shelfPos.x, shelfPos.y, camPos.z);
    }
}
