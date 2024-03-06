using System.Globalization;
using System.Linq;
using System.Xml;
using UnityEngine;

public class TransformProjector : MonoBehaviour
{
    public GameObject projector;
    public GameObject kinect;
    // Start is called before the first frame update
    void Start()    
    {
        XmlDocument doc = new XmlDocument();
        doc.Load("c:/Users/strof/Programovani/Github/arcor2_sar/Calibration/calibration_result.xml");

        XmlNode transVectorNode = doc.DocumentElement.SelectSingleNode("/opencv_storage/translation/data");
        string transVectorData = transVectorNode.InnerText;
        string[] parsedTransVector = transVectorData.Trim().Split('\n', ' ');
        parsedTransVector = parsedTransVector.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        projector.transform.position = new Vector3(
            (float.Parse(parsedTransVector[0], CultureInfo.InvariantCulture.NumberFormat) * (-1) / 100),
            (float.Parse(parsedTransVector[1], CultureInfo.InvariantCulture.NumberFormat) * (-1) / 100),
            (float.Parse(parsedTransVector[2], CultureInfo.InvariantCulture.NumberFormat) * (-1) / 100)
        ) + kinect.transform.position;

        XmlNode rotationMatrixNode = doc.DocumentElement.SelectSingleNode("/opencv_storage/rotation/data");
        string rotationMatrixData = rotationMatrixNode.InnerText;
        string[] parsedRotationMatrix = rotationMatrixData.Trim().Split('\n', ' ');
        parsedRotationMatrix = parsedRotationMatrix.Where(x => !string.IsNullOrEmpty(x)).ToArray();
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

        projector.transform.rotation = quaternionRotation;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
