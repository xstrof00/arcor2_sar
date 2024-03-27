using System;
using UnityEngine;
using static System.Math;

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

        double kinectFovVertical = CalculateKinectFovVertical(kinectCalibrationData);
        Debug.Log(kinectFovVertical);
        kinect.GetComponent<Camera>().fieldOfView = (float)kinectFovVertical;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    double CalculateKinectFovVertical(KinectCalibrationData kinectCalibrationData)
    {
        double[] kinectFov = new double[2];
        float fy = kinectCalibrationData.intrinsics[5];
        float resolutionHeight = kinectCalibrationData.resolution[0];

        double kinectFovVertical = 2 * Atan(resolutionHeight / (2 * fy));

        kinectFov[1] = kinectFovVertical;

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
