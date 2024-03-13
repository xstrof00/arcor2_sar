using System.Globalization;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransformProjector : MonoBehaviour
{
    public GameObject projector;
    public GameObject kinect;
    public TextAsset xmlFile;
    public GameObject arUcoMarker;

    // Start is called before the first frame update
    void Start()
    {
        //XmlDocument xmlDoc = LoadXmlDoc();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlFile.text);

        SetProjectorPosition(xmlDoc);
        SetProjectorRotation(xmlDoc);

        Camera camera = projector.GetComponent<Camera>();
        Screen.SetResolution(1920, 1080, true);

        arUcoMarker.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*XmlDocument LoadXmlDoc()
    {
        XmlDocument doc = new XmlDocument();
        doc.Load("../Calibration/calibration_result.xml");
        return doc;
    }*/

    void SetProjectorPosition(XmlDocument xmlDoc)
    { 
        XmlNode transVectorNode = xmlDoc.DocumentElement.SelectSingleNode("/opencv_storage/translation/data");
        string[] parsedTransVector = GetStringFromXmlNode(transVectorNode);

        Vector3 projectorTransVector = new Vector3(
            (float.Parse(parsedTransVector[0], CultureInfo.InvariantCulture.NumberFormat) * (-1) / 100),
            (float.Parse(parsedTransVector[1], CultureInfo.InvariantCulture.NumberFormat) * (-1) / 100),
            (float.Parse(parsedTransVector[2], CultureInfo.InvariantCulture.NumberFormat) * (-1) / 100)
        ) + kinect.transform.position;

        projector.transform.position = projectorTransVector;
    }

    void SetProjectorRotation(XmlDocument xmlDoc)
    {
        XmlNode rotationMatrixNode = xmlDoc.DocumentElement.SelectSingleNode("/opencv_storage/rotation/data");
        string[] parsedRotationMatrix = GetStringFromXmlNode(rotationMatrixNode);

        Matrix4x4 rotationMatrix = new Matrix4x4();
        rotationMatrix.SetRow(0, new Vector4(float.Parse(parsedRotationMatrix[0], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedRotationMatrix[1], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedRotationMatrix[2], CultureInfo.InvariantCulture.NumberFormat),
                                              0f));
        rotationMatrix.SetRow(1, new Vector4(float.Parse(parsedRotationMatrix[3], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedRotationMatrix[4], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedRotationMatrix[5], CultureInfo.InvariantCulture.NumberFormat),
                                              0f));
        rotationMatrix.SetRow(2, new Vector4(float.Parse(parsedRotationMatrix[6], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedRotationMatrix[7], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedRotationMatrix[8], CultureInfo.InvariantCulture.NumberFormat),
                                              0f));
        rotationMatrix.SetRow(3, new Vector4(0f, 0f, 0f, 0f));

        Vector3 forward = rotationMatrix.GetColumn(2);
        Vector3 up = rotationMatrix.GetColumn(1);

        Quaternion quaternionRotation = Quaternion.LookRotation(forward, up);

        projector.transform.rotation = quaternionRotation * kinect.transform.rotation;
    }

    string[] GetStringFromXmlNode(XmlNode xmlNode)
    {
        string data = xmlNode.InnerText;
        string[] parsedData = data.Trim().Split('\n', ' ');
        parsedData = parsedData.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        return parsedData;
    }
}
