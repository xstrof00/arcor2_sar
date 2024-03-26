using UnityEngine;

public class TransformKinect : MonoBehaviour
{
    public GameObject kinect;
    public TextAsset xmlFile;
    public TextAsset jsonFile;

    // Start is called before the first frame update
    void Start()
    {
        KinectCalibrationData kinectCalibrationData = new KinectCalibrationData(xmlFile, jsonFile);

        SetKinectPosition(kinectCalibrationData);
        SetKinectRotation(kinectCalibrationData);

        //TODO - change fieldOfView by calculating it 
        //https://stackoverflow.com/questions/39992968/how-to-calculate-field-of-view-of-the-camera-from-camera-intrinsic-matrix
        //kinect.GetComponent<Camera>().fieldOfView = 50;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetKinectPosition(KinectCalibrationData kinectCalibrationData)
    {
        Vector3 kinectTranslation = new Vector3(
            kinectCalibrationData.pose.position.x * -1 * 10,
            kinectCalibrationData.pose.position.y * -1 * 10,
            kinectCalibrationData.pose.position.z * -1 * 10
        );

        kinect.transform.position = kinectTranslation;
    }

    void SetKinectRotation(KinectCalibrationData kinectCalibrationData)
    {
        Quaternion kinectRotation = new Quaternion(
            kinectCalibrationData.pose.orientation.x,
            kinectCalibrationData.pose.orientation.y,
            kinectCalibrationData.pose.orientation.z,
            kinectCalibrationData.pose.orientation.w
        );

        Quaternion flipRotationByXZ = Quaternion.Euler(180f, 0f, 180f);
        kinect.transform.rotation = kinectRotation * flipRotationByXZ;
    }
}
