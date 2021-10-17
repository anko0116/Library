using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class ReadXML : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // http://unitynoobs.blogspot.com/2011/02/xml-loading-data-from-xml-file.html
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = false; 
        XmlReader reader = XmlReader.Create("Assets\\Scripts\\Day1xml.xml", readerSettings);
        while (reader.Read()) { 
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    // Do Something
                    print(reader.Name);
                    break;
                case XmlNodeType.Text:
                    print(reader.Value);
                    break;
                case XmlNodeType.Comment:
                    //print("COMMENT - " + reader.Value);
                    break;
                default:
                    // Do something
                    //print(reader.Value);
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
