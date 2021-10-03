using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookInit : MonoBehaviour
{
    // Attached to MainCamera
    MouseLMB mouseScript;
    bool booksRotated;
    void Start()
    {
        mouseScript = gameObject.GetComponent<MouseLMB>();
        booksRotated = false;
    }

    void Update() {
        if (!booksRotated) {
            booksRotated = true;
            GameObject[] books = GameObject.FindGameObjectsWithTag("Book");
            foreach (GameObject book in books) {
                mouseScript.RotateBook(book);
            }
        }
    }
}
