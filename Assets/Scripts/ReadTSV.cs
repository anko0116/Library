using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Attached to the TMP object for dialogue/text in each scene

public class ReadTSV : MonoBehaviour
{
    public static TextAsset tsvFile;
    public static List<string> tsvLines;
    public static int lineCnt;
    public static List<string> currLine;
    public static string sceneName;
    public static ButtonGameplay playScript;
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
            else if (lineType == "Sprite") {
                StartCoroutine(LoadSprite());
            }
            else if (lineType == "Animation") {
                StartCoroutine(LoadAnim());
            }
            else if (lineType == "Gameplay") {
                StartCoroutine(LoadGameplay());
            }
        }
    }

    IEnumerator LoadScene() {
        SceneManager.LoadScene(currLine[1]);
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == currLine[1]);

        ReadAndParseLine();
        while (currLine[0] != "Text") {
            if (currLine[0] == "Sprite") {
                //yield return LoadSprite();
            }
            // FIXME: Will add more soon!!!
            ReadAndParseLine();
        }
        yield return LoadText(true);
        tsvLock = false;
        //yield return null;
        //SceneManager.LoadScene("LibraryScene");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    IEnumerator LoadText(bool continuation=false) {
        // Find TextMeshPro for dialogue text
        inputField = gameObject.GetComponent<TextMeshProUGUI>();
        // Shove that text in TMP
        inputField.text = currLine[2];

        // Find TextMeshPro for speaker name
        if (currLine[1] != "/") {
            Transform nameTransf = gameObject.transform.parent.Find("SpeakerName");
            TextMeshProUGUI nameField = nameTransf.GetComponent<TextMeshProUGUI>();
            if (currLine[1] == "-") {
                nameField.text = "";
            }
            else {
                nameField.text = currLine[1];
            }
        }

        // Find image component
        if (currLine[3] != "\r") {
            Transform imageTransf = gameObject.transform.parent.Find("SpaceImage");
            Image image = imageTransf.GetComponent<Image>();
            // Shove that image
            Sprite[] spr = Resources.LoadAll<Sprite>("IntroArts/");
            image.sprite = spr[0];
            var tempColor = image.color;
            tempColor.a = 1f;
            image.color = tempColor;
        }

        // Find dialogue button
        Transform buttonTransf = gameObject.transform.parent.Find("DialogueButton");
        Button button = buttonTransf.GetComponent<Button>();
        bool clicked = false;
        button.onClick.AddListener(() => {clicked = true;});
        // Wait until button is pressed to continue to next dialogue
        // All tsv line reading runs until IT HITS HERE AT THE DIALOGUE/TEXT LINE - COOL. DIDN'T EVEN INTEND IT
        yield return new WaitUntil(() => clicked);
        if (!continuation) { 
            tsvLock = false;
        }
    }

    IEnumerator LoadSprite() {
        // FIXME: currently, this can only hanlde putting in character sprites.
        // Will need to change if I want to spawn other sprites.

        // Load Sprite from Resources
        Sprite spr = Resources.Load<Sprite>("Arts/" + currLine[1]);
        // Find NPCSpot GameObject to place Sprite
        SpriteRenderer sprSpot = 
            GameObject.FindWithTag("NPCSpot").GetComponent<SpriteRenderer>();
        sprSpot.sprite = spr;

        tsvLock = false;
        yield return null;
    }

    IEnumerator LoadGameplay() {
        // Find Gameplay Script
        playScript = gameObject.transform.parent.Find("GameplayButton")
            .GetComponent<ButtonGameplay>();
        // Find DialogueButton
        Transform buttonTransf = gameObject.transform.parent.Find("DialogueButton");
        Image buttonImg = buttonTransf.GetComponent<Image>();
        if (!playScript.slide) {
            buttonImg.sprite = Resources.Load<Sprite>("Arts/give");
        }
        else {
            buttonImg.sprite = Resources.Load<Sprite>("Arts/next");
        }
        playScript.slide = !playScript.slide;

        // Wait until correct books are on table and press give button
        bool booksGiven = false;
        yield return new WaitUntil(() => booksGiven);

        tsvLock = false;
        yield return null;
    }

    IEnumerator LoadAnim() {
        tsvLock = false;
        yield return null;
    }

}
