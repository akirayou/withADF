using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tango;

//
//UDP transmition datas 
//member names must be short. for short JSON string
//
[System.Serializable]
public class Command
{
    public string cmd;
    public string arg;
}
[System.Serializable]
public struct ArgPose
{
    public string label;
    public Vector3 p;
    public Quaternion r;
}
[System.Serializable]
public struct ArgSense
{
    public int c;
    public int v;
}

public class UDPGeneration : MonoBehaviour {

	public GameObject UDPCommGameObject;
    public bool EnableCam = true;
    private GameObject Robo,LocalGoal;
    static private ArgSense sensData =new ArgSense();
    static private bool newSense = false;
    public GameObject Camera;
    public GameObject SensObj;
    public ViewTangoInfo vieTangoInfo;
    Tango.TangoCoordinateFramePair framePair;

    void Start () {
        framePair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION;
        framePair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;
        if (UDPCommGameObject == null) {
			Debug.Log ("ERR UDPGEN: UDPSender is required. Self-destructing.");
			Destroy (this);
		}
        Robo = GameObject.Find("Robo");
        LocalGoal = GameObject.Find("LocalGoal");
    }


    public void SendCmd(Command cmd)
    {
        var dataBytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(cmd));
        UDPCommunication comm = UDPCommGameObject.GetComponent<UDPCommunication>();
        comm.SendUDPMessage(dataBytes);
    }
    public void SendPose(ArgPose pose)
    {
        Command cmd = new Command();
        cmd.cmd = "pose";
        cmd.arg = JsonUtility.ToJson(pose);
        SendCmd(cmd);
    }

    static ArgPose roboPose=new ArgPose();
    private int sensCount=0;
    static ArgPose localGoalPose = new ArgPose();
    private bool poseOk = false;
    void Update()
    {


        if (!vieTangoInfo.UseADF) framePair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE;
        ArgPose pose = new ArgPose();
        pose.label = "Tcam";
        TangoPoseData p = new TangoPoseData();
        PoseProvider.GetPoseAtTime(p, 0, framePair);
        if (p.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID ) poseOk = true;
        else poseOk = false;
        vieTangoInfo.PoseOk = poseOk;
        if (EnableCam && poseOk)
        {
            pose.p.x = (float)p.translation.x;
            pose.p.y = (float)p.translation.y;
            pose.p.z = (float)p.translation.z;
            pose.r.x = (float)p.orientation.x;
            pose.r.y = (float)p.orientation.y;
            pose.r.z = (float)p.orientation.z;
            pose.r.w = (float)p.orientation.w;

            // Debug.Log("camera ROLL"+pose.r.ToString());
            SendPose(pose);
        }

        Robo.transform.position = new Vector3(roboPose.p.x, roboPose.p.y, roboPose.p.z);
        LocalGoal.transform.position = new Vector3(localGoalPose.p.x, localGoalPose.p.y, localGoalPose.p.z);

        // Robo.transform.Translate(pose.p.x, pose.p.y, pose.p.z);
        // Debug.Log(Robo.transform.position);
        // Debug.Log(roboPose.p.x.ToString()+ roboPose.p.y.ToString()+roboPose.p.z.ToString());
        // Robo.rotation.Set(pose.r.x, pose.r.y, pose.r.z,pose.r.w);
        if (newSense)
        {
            newSense = false;
            sensCount++;
            Debug.Log("SENS:"+sensData.v.ToString());
            if (sensCount % 2==0)
            {
                GameObject so = Instantiate(SensObj, Robo.transform.position, Robo.transform.rotation);
                float v = sensData.v / 1000.0f;
                v = Mathf.Pow(v, 0.2f);

                if (v > 1) v = 1;
                Color col = Color.HSVToRGB(0.75f - v * 0.75f, 1, 1);
                col.a = 0.4f;
                so.GetComponent<Renderer>().material.color = col;
                Destroy(so, 60.0f);
            }

        }

    }

    void CmdEnable(string arg, bool enable)
    {
        if (arg == "cam") EnableCam = enable;
    }
    void CmdPose(string json)
    {
        ArgPose pose = JsonUtility.FromJson<ArgPose>(json);
        if (pose.label == "Trb")
        {
            roboPose = pose;
            //Debug.Log("set pose"+ roboPose.p.x.ToString() + roboPose.p.y.ToString() + roboPose.p.z.ToString());
        }
        if (pose.label == "Tlg")
        {
            localGoalPose = pose;
            //Debug.Log("set pose"+ roboPose.p.x.ToString() + roboPose.p.y.ToString() + roboPose.p.z.ToString());
        }
    }
    public void Recv(string fromIP, string fromPort, byte[] data)
    {
        string json = System.Text.Encoding.UTF8.GetString(data);
        Command cmd = JsonUtility.FromJson<Command>(json);
        if (cmd.cmd == "enable") CmdEnable(cmd.arg,true);
        if (cmd.cmd == "disable") CmdEnable(cmd.arg,false);
        if (cmd.cmd == "pose") CmdPose(cmd.arg);

    }
    public void RecvSens(string fromIP, string fromPort, byte[] data)
    {
        string json = System.Text.Encoding.UTF8.GetString(data);
        ArgSense sens = JsonUtility.FromJson<ArgSense>(json);


        sensData = sens;
        newSense = true;
    }
}
