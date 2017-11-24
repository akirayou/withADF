using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetGoal : MonoBehaviour {
    public ViewTangoInfo tinfo;
    public UDPGeneration udp;
    // Use this for initialization
    private ArgPose ap = new ArgPose();
    void Start () {
        ap.label = "Tgoal";
        ap.r.x = 0;
        ap.r.y = 0;
        ap.r.z = 0;
        ap.r.w = 1;

    }

    // Update is called once per frame
    void Update()
    {
        if (!tinfo.PoseOk) return;
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 cpos = Input.mousePosition;
            RaycastHit hitInfo;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(cpos), out hitInfo, Mathf.Infinity, 1 << LayerMask.NameToLayer("TangoMesh")))
            {
                this.transform.position = hitInfo.point + new Vector3(0, 1, 0);
                ap.p= this.transform.position;

                udp.SendPose(ap);

            }
            else
            {
                AndroidHelper.ShowAndroidToastMessage("Not Recognized Area",AndroidHelper.ToastLength.SHORT);   
            }
        }
       
    }
}
