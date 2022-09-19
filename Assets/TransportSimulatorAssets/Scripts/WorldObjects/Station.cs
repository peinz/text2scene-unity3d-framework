using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Station : MonoBehaviour {

    public Text StationText;
    public Dropdown TransportLineDropdown;

    List<string> transportLines = new List<string>();

    public void SetName(string name)
    {
        StationText.text = name;
    }
    public void AddTransportLines(List<string> transportLines)
    {
        var newLines = new List<string>();
        foreach(var line in transportLines){
            if(!this.transportLines.Contains(line)) {
                newLines.Add(line);
            }
        }
        TransportLineDropdown.AddOptions(newLines);
        this.transportLines.AddRange(newLines);
    }
    
} 