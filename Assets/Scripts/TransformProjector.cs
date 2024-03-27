using UnityEngine;
using static System.Math;

public class TransformProjector : MonoBehaviour
{
    public GameObject projector;
    public GameObject kinect;
    public TextAsset xmlFile;
    public GameObject arUcoMarker;

    // Start is called before the first frame update
    void Start()
    {
        ProjectorCalibrationData projCalibrationData = new ProjectorCalibrationData(xmlFile);

        SetProjectorPosition(projCalibrationData);
        SetProjectorRotation(projCalibrationData);
        double projectorFovVertical = CalculateProjectorFovVertical(projCalibrationData);
        projector.GetComponent<Camera>().fieldOfView = (float)projectorFovVertical;

        Screen.SetResolution(1920, 1080, true);

        arUcoMarker.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    double CalculateProjectorFovVertical(ProjectorCalibrationData projectorCalibrationData)
    {
        double[] projectorFov = new double[2];
        float fy = projectorCalibrationData.intrinsics[5];
        float resolutionHeight = projectorCalibrationData.height;

        double projectorFovVertical = 2 * Atan(resolutionHeight / (2 * fy));

        projectorFov[1] = projectorFovVertical;

        double verticalFovInDegrees = ConvertRadiansToDegrees(projectorFovVertical);
        return verticalFovInDegrees;
    }

    double ConvertRadiansToDegrees(double radians)
    {
        double degrees = (radians * 180 / PI);
        return degrees;
    }

    void SetProjectorPosition(ProjectorCalibrationData projCalibrationData)
    { 
        projector.transform.position = projCalibrationData.position + kinect.transform.position;
    }

    void SetProjectorRotation(ProjectorCalibrationData projCalibrationData)
    {
        Vector3 forward = projCalibrationData.rotation.GetColumn(2);
        Vector3 up = projCalibrationData.rotation.GetColumn(1);

        Quaternion projectorRotation = Quaternion.LookRotation(forward, up);

        Quaternion flipRotation = Quaternion.Euler(180f, 0f, 180f);

        Quaternion inverseKinectRotation = Quaternion.Inverse(kinect.transform.rotation);
        Quaternion inverseProjectorRotation = Quaternion.Inverse(projectorRotation);
        projector.transform.rotation = projectorRotation * kinect.transform.rotation;
    }
}
