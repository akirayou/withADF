using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tango;

public class ViewTangoInfo : MonoBehaviour {
    public Text text;
    public UDPGeneration udpGene;
    public bool PoseOk=false;
    public bool UseADF = true;
	// Use this for initialization
	void Start () {
        text.text = "";

	}
	
	// Update is called once per frame
	void Update () {
        text.text = "";
        if (!UseADF) text.text += "NoADF ";
        if (!PoseOk) text.text += "Tango is not ready "; 


    }
}
