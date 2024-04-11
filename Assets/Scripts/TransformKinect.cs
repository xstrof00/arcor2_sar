using Base;
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
        kinect.GetComponent<Camera>().fieldOfView = (float)kinectFovVertical;

        TestConnectToServer();
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

    async void TestConnectToServer()
    {
        WebsocketManager.Instance.ConnectToServer("192.168.104.100", 6789);
        IO.Swagger.Model.SystemInfoResponseData systemInfo;
        try
        {
            systemInfo = await WebsocketManager.Instance.GetSystemInfo();
        }
        catch(RequestFailedException ex)
        {
            Debug.Log(ex.Message);
        }
    }
}
