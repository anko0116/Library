using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookInit : MonoBehaviour
{
    MouseLMB mouseScript;
    // Start is called before the first frame update
    void Start()
    {
        mouseScript = GetComponent<MouseLMB>();
        mouseScript.RotateBook();
        GameObject[] books = GameObject.FindGameObjectsWithTag("Book");
        foreach (GameObject book in books) {
            print(book);
            mouseScript.RotateBook(book);
        }
    }
}
