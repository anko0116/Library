using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ReadCSV : MonoBehaviour
{
    public static TextAsset csvFile;
    TextMeshProUGUI inputField;
    // Start is called before the first frame update
    void Start() {
    // https://yarnspinner.dev/docs/tutorial
    // https://www.youtube.com/watch?v=unxAhAsqJko
    //https://forum.unity.com/threads/how-to-modify-the-text-of-a-textmeshpro-input-field.765770/
    // https://www.youtube.com/watch?v=CE9VOZivb3I - brackeys transition scene

        csvFile = Resources.Load<TextAsset>("Day1");
        //print(csvFile.ToString());

        inputField = gameObject.GetComponent<TextMeshProUGUI>();
        //string text = inputField.text;
        print(inputField.text);
    }

    // Update is called once per frame
    void Update()
    {
        //SceneManager.LoadScene("LibraryScene");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
