using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tango;


public class ADFSelector : MonoBehaviour, ITangoLifecycle
{
    AreaDescription[] list=null;
    public UnityEngine.UI.Dropdown dropdown;
    public TangoApplication tangoApp;
    public ViewTangoInfo viewTangoInfo;
    public TangoPoseController poseCont;
    // Use this for initialization
    void Start()
    {
        dropdown.options.Clear();
        tangoApp.Register(this);
        tangoApp.EnableAreaDescriptions = true;
        tangoApp.Enable3DReconstruction = true;
        //tangoApp.EnableCloudADF = true;
        tangoApp.EnableDepth = true;
        tangoApp.EnableMotionTracking = true;

        
        tangoApp.RequestPermissions();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
       // AndroidHelper.StartTangoPermissionsActivity("Dataset Read/Write");

        dropdown.options.Clear();
        if (!permissionsGranted) return;
        dropdown.options.Add(new Dropdown.OptionData { text = "No ADF" });
        list = AreaDescription.GetList();
        if(list!=null)foreach (AreaDescription ad in list)
        {
            dropdown.options.Add(new Dropdown.OptionData( ad.GetMetadata().m_name + "_"+ ad.m_uuid));
        }
        dropdown.RefreshShownValue();
    }

    public void OnGo()
    {
        int no = dropdown.value - 1;

        Debug.Log("SELECTED ADF No " + no.ToString());

        if (no < 0)
        {
            viewTangoInfo.UseADF = false;
            poseCont.m_baseFrameMode = TangoPoseController.BaseFrameSelectionModeEnum.USE_START_OF_SERVICE;
            tangoApp.UseADFOn3dr = false;

            
            tangoApp.Startup(null);
        }
        else
        {
            poseCont.m_baseFrameMode = TangoPoseController.BaseFrameSelectionModeEnum.USE_AREA_DESCRIPTION;
            viewTangoInfo.UseADF = true;
            tangoApp.UseADFOn3dr = true;
            tangoApp.Startup(list[no]);
        }

        this.gameObject.SetActive( false);


    }




    public void OnTangoServiceConnected()
    {//none
    }
    public void OnTangoServiceDisconnected()
    {//none
    }

}