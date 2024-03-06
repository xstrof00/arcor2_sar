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

        Vector3 kinectTranslation = new Vector3(
            kinectData.pose.position.x * -1 * 10,
            kinectData.pose.position.y * -1 * 10,
            kinectData.pose.position.z * -1 * 10
        );

        Quaternion kinectRotation = new Quaternion(
            kinectData.pose.orientation.x,
            kinectData.pose.orientation.y,
            kinectData.pose.orientation.z,
            kinectData.pose.orientation.w
        );

        kinect.transform.position = kinectTranslation;
        kinect.transform.rotation = kinectRotation;

        Quaternion flipRotation = Quaternion.Euler(180f, 0f, 0f);

        kinect.transform.rotation *= flipRotation;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
