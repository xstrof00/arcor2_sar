using UnityEngine;

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

        Screen.SetResolution(1920, 1080, true);

        arUcoMarker.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

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
