using System.Globalization;
using System.Linq;
using System.Xml;
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

public class KinectCalibrationData
{
    public Pose pose { get; private set; }
    public Matrix4x4 intrinsics { get; private set; }
    public float[] distortion { get; private set; }

    public KinectCalibrationData(TextAsset xmlFile, TextAsset jsonFile)
    {
        KinectData kinectData = JsonUtility.FromJson<KinectData>(jsonFile.text);
        XmlDocument xmlDoc = LoadXmlDoc(xmlFile);

        pose = kinectData.pose;
        intrinsics = ReadKinectIntrinsics(xmlDoc);
        distortion = ReadKinectDistortion(xmlDoc);
    }

    XmlDocument LoadXmlDoc(TextAsset xmlFile)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlFile.text);
        return xmlDoc;
    }

    Matrix4x4 ReadKinectIntrinsics(XmlDocument xmlDoc)
    {
        Matrix4x4 kinectIntrinsics = new Matrix4x4();
        XmlNode instrinsicsMatrixNode = xmlDoc.DocumentElement.SelectSingleNode("/opencv_storage/cam_int/data");
        string[] parsedIntrinsicsMatrix = GetStringFromXmlNode(instrinsicsMatrixNode);
        kinectIntrinsics.SetRow(0, new Vector4(float.Parse(parsedIntrinsicsMatrix[0], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedIntrinsicsMatrix[1], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedIntrinsicsMatrix[2], CultureInfo.InvariantCulture.NumberFormat),
                                              0f));
        kinectIntrinsics.SetRow(1, new Vector4(float.Parse(parsedIntrinsicsMatrix[3], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedIntrinsicsMatrix[4], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedIntrinsicsMatrix[5], CultureInfo.InvariantCulture.NumberFormat),
                                              0f));
        kinectIntrinsics.SetRow(2, new Vector4(float.Parse(parsedIntrinsicsMatrix[6], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedIntrinsicsMatrix[7], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedIntrinsicsMatrix[8], CultureInfo.InvariantCulture.NumberFormat),
                                              0f));
        kinectIntrinsics.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
        return kinectIntrinsics;
    }

    float[] ReadKinectDistortion(XmlDocument xmlDoc)
    {
        float[] kinectDistortion = new float[5];
        XmlNode distortionMatrixNode = xmlDoc.DocumentElement.SelectSingleNode("/opencv_storage/cam_dist/data");
        string[] parsedDistortionCoeficients = GetStringFromXmlNode(distortionMatrixNode);
        for (int i = 0; i < parsedDistortionCoeficients.Length; i++)
        {
            kinectDistortion[i] = float.Parse(parsedDistortionCoeficients[i], CultureInfo.InvariantCulture.NumberFormat);
        }
        return kinectDistortion;
    }

    string[] GetStringFromXmlNode(XmlNode xmlNode)
    {
        string data = xmlNode.InnerText;
        string[] parsedData = data.Trim().Split('\n', ' ');
        parsedData = parsedData.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        return parsedData;
    }
}
