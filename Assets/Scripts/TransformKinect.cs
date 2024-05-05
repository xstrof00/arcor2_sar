//author: Jakub Štrof

using Base;
using UnityEngine;
using static System.Math;

public class TransformKinect : Singleton<TransformKinect>
{
    public GameObject kinect;
    public TextAsset xmlFile;
    public TextAsset jsonFile;

    // Start is called before the first frame update
    void Start()
    {
        kinect = GameObject.Find("Kinect");
        xmlFile = Resources.Load("calibration_result") as TextAsset;
        jsonFile = Resources.Load("kinectCalibrationData") as TextAsset;

        KinectCalibrationData kinectCalibrationData = new KinectCalibrationData(xmlFile, jsonFile);
        Camera cam = kinect.GetComponent<Camera>();

        SetKinectPosition(kinectCalibrationData);
        SetKinectRotation(kinectCalibrationData);

        double kinectFovVertical = CalculateKinectFovVertical(kinectCalibrationData);
        cam.fieldOfView = (float)kinectFovVertical;

        GameManager.Instance.ConnectToServer();
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    double CalculateKinectFovVertical(KinectCalibrationData kinectCalibrationData)
    {
        float fy = kinectCalibrationData.intrinsics[5];
        float resolutionHeight = kinectCalibrationData.resolution[0];

        double kinectFovVertical = 2 * Atan(resolutionHeight / (2 * fy));

        double verticalFovInDegrees = ConvertRadiansToDegrees(kinectFovVertical);
        return verticalFovInDegrees;
    }

    double ConvertRadiansToDegrees(double radians)
    {
        double degrees = (radians * 180 / PI);
        return degrees;
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
