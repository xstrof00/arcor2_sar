//author: Jakub Štrof

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
        projector = GameObject.Find("Projector");
        kinect = GameObject.Find("Kinect");
        xmlFile = Resources.Load("calibration_result") as TextAsset;
        arUcoMarker = GameObject.Find("ArUco");

        ProjectorCalibrationData projCalibrationData = new ProjectorCalibrationData(xmlFile);
        Camera cam = projector.GetComponent<Camera>();

        SetProjectorPosition(projCalibrationData);
        SetProjectorRotation(projCalibrationData);
        SetPerspectiveMatrix(projCalibrationData, cam);

        double projectorFovVertical = CalculateProjectorFovVertical(projCalibrationData);
        cam.fieldOfView = (float)projectorFovVertical;

        Screen.SetResolution(1920, 1080, true);

        arUcoMarker.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    double CalculateProjectorFovVertical(ProjectorCalibrationData projCalibrationData)
    {
        double[] projectorFov = new double[2];
        float fy = projCalibrationData.intrinsics[5];
        float resolutionHeight = projCalibrationData.height;

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

        projector.transform.rotation = projectorRotation * kinect.transform.rotation;
    }

    void SetPerspectiveMatrix(ProjectorCalibrationData projCalibrationData, Camera cam)
    {
        //Following code in this function was adapted from: https://kamino.hatenablog.com/entry/unity-import-opencv-camera-params

        Matrix4x4 PerspectiveMatrix()
        {
            var m = new Matrix4x4();
            m[0, 0] = 2 * projCalibrationData.intrinsics[0, 0] / projCalibrationData.width;
            m[0, 1] = 0;
            m[0, 2] = 1 - 2 * projCalibrationData.intrinsics[0, 2] / projCalibrationData.width;
            m[0, 3] = 0;

            m[1, 0] = 0;
            m[1, 1] = 2 * projCalibrationData.intrinsics[1, 1] / projCalibrationData.height;
            m[1, 2] = -1 + 2 * projCalibrationData.intrinsics[1, 2] / projCalibrationData.height;
            m[1, 3] = 0;

            m[2, 0] = 0;
            m[2, 1] = 0;
            m[2, 2] = -(cam.farClipPlane + cam.nearClipPlane) / (cam.farClipPlane - cam.nearClipPlane);
            m[2, 3] = -2 * cam.farClipPlane * cam.nearClipPlane / (cam.farClipPlane - cam.nearClipPlane);

            m[3, 0] = 0;
            m[3, 1] = 0;
            m[3, 2] = -1;
            m[3, 3] = 0;
            return m;
        }

        cam.projectionMatrix = PerspectiveMatrix();
    }
}
