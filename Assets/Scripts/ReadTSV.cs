using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ReadTSV : MonoBehaviour
{
    public static TextAsset tsvFile;
    public static List<string> tsvLines;
    public static int lineCnt;
    public static List<string> currLine;
    public static string sceneName;
    TextMeshProUGUI inputField;
    bool tsvLock;

    void Start() {
    // https://yarnspinner.dev/docs/tutorial
    // https://www.youtube.com/watch?v=unxAhAsqJko
    // https://forum.unity.com/threads/how-to-modify-the-text-of-a-textmeshpro-input-field.765770/
    // https://www.youtube.com/watch?v=CE9VOZivb3I - brackeys transition scene
    tsvLock = false;
        if (!tsvFile && !tsvLock) {
            tsvLock = true;
            tsvFile = Resources.Load<TextAsset>("Day1");
            tsvLines = new List<string>(tsvFile.text.Split('\n'));
            lineCnt = 0;
            tsvLock = false;
        }
    }

    void Update() {
        ParseTSVLine();
    }

    void ReadAndParseLine() {
        currLine = new List<string>(tsvLines[lineCnt].Split('\t'));
        lineCnt += 1;
    }

    void ParseTSVLine() {
        // Split current line
        if (!tsvLock && lineCnt < tsvLines.Count) {
            tsvLock = true;
            ReadAndParseLine();

            // FIXME: temp fix
            if (currLine[0] == "") {
                tsvLock = false;
                return;
            }

            string lineType = currLine[0];
            if (lineType == "Scene") {
                StartCoroutine(LoadScene());
            }
            else if (lineType == "Text") {
                StartCoroutine(LoadText());
            }
            else if (lineType == "Animation") {
                StartCoroutine(LoadAnim());
            }
        }
    }

    IEnumerator LoadScene() {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(currLine[1]);
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == currLine[1]);
        ReadAndParseLine();
        print("Loadingtext");
        yield return LoadText(true);
        tsvLock = false;
        //yield return null;
        //SceneManager.LoadScene("LibraryScene");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    IEnumerator LoadText(bool continuation=false) {
        print("eee");
        // Find the TextMeshPro for dialogue text
        inputField = gameObject.GetComponent<TextMeshProUGUI>();
        // Shove that text in TMP
        inputField.text = currLine[2];
        print("1");
        // Find image component
        Transform imageTransf = gameObject.transform.parent.Find("SpaceImage");
        Image image = imageTransf.GetComponent<Image>();
        // Shove that image
        Sprite[] spr = Resources.LoadAll<Sprite>("IntroArts/");
        image.sprite = spr[0];
        var tempColor = image.color;
        tempColor.a = 1f;
        image.color = tempColor;

        // Find dialogue button
        Transform buttonTransf = gameObject.transform.parent.Find("DialogueButton");
        Button button = buttonTransf.GetComponent<Button>();
        bool clicked = false;
        button.onClick.AddListener(() => {clicked = true;});
        // Wait until button is pressed to continue to next dialogue
        print("hello");
        yield return new WaitUntil(() => clicked);
        if (!continuation) { 
            tsvLock = false;
        }
    }

    IEnumerator LoadAnim() {
        tsvLock = false;
        yield return null;
    }

}
