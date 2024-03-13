using UnityEngine;

[System.Serializable]
public class KinectData
{
    public Pose pose;
    public double quality;
}

[System.Serializable]
public class Pose
{
    public Orientation orientation;
    public Position position;
}

[System.Serializable]
public class Orientation
{
    public float w;
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;
}

public class TransformKinect : MonoBehaviour
{
    public GameObject kinect;
    public TextAsset jsonFile;

    // Start is called before the first frame update
    void Start()
    {
        KinectData kinectData = JsonUtility.FromJson<KinectData>(jsonFile.text);

        SetKinectPosition(kinectData);
        SetKinectRotation(kinectData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void SetKinectPosition(KinectData kinectData)
    {
        Vector3 kinectTranslation = new Vector3(
            kinectData.pose.position.x * -1 * 10,
            kinectData.pose.position.y * -1 * 10,
            kinectData.pose.position.z * -1 * 10
        );
        kinect.transform.position = kinectTranslation;
    }

    void SetKinectRotation(KinectData kinectData)
    {
        Quaternion kinectRotation = new Quaternion(
            kinectData.pose.orientation.x,
            kinectData.pose.orientation.y,
            kinectData.pose.orientation.z,
            kinectData.pose.orientation.w
        );

        Quaternion flipRotation = Quaternion.Euler(180f, 0f, 180f);

        kinect.transform.rotation = kinectRotation * flipRotation;
    }
}
