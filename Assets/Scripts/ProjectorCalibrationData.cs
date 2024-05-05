//author: Jakub Štrof

using System.Xml;
using UnityEngine;
using System.Globalization;
using System.Linq;

public class ProjectorCalibrationData
{
	public int height { get; private set; }
	public int width { get; private set; }
    public Matrix4x4 intrinsics { get; private set; }
    public float[] distortion { get; private set; }
	public Matrix4x4 rotation { get; private set; }
	public Vector3 position { get; private set; }

	public ProjectorCalibrationData(TextAsset xmlFile)
    { 
        XmlDocument xmlDoc = LoadXmlDoc(xmlFile);

        SetProjectorHeight();
        SetProjectorWidth();
        intrinsics = ReadProjectorIntrinsics(xmlDoc);
        distortion = ReadProjectorDistortion(xmlDoc);
		rotation = ReadProjectorRotation(xmlDoc);
        position = ReadProjectorPosition(xmlDoc);
	}

	XmlDocument LoadXmlDoc(TextAsset xmlFile)
	{
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlFile.text);
        return xmlDoc;
    }

    void SetProjectorHeight()
    {
        height = 1080;
    }

    void SetProjectorWidth()
    {
        width = 1920;
    }

    Matrix4x4 ReadProjectorIntrinsics(XmlDocument xmlDoc)
    {
        Matrix4x4 projectorInstrinsics = new Matrix4x4();
        XmlNode instrinsicsMatrixNode = xmlDoc.DocumentElement.SelectSingleNode("/opencv_storage/proj_int/data");
        string[] parsedIntrinsicsMatrix = GetStringFromXmlNode(instrinsicsMatrixNode);
        projectorInstrinsics.SetRow(0, new Vector4(float.Parse(parsedIntrinsicsMatrix[0], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedIntrinsicsMatrix[1], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedIntrinsicsMatrix[2], CultureInfo.InvariantCulture.NumberFormat),
                                              0f));
        projectorInstrinsics.SetRow(1, new Vector4(float.Parse(parsedIntrinsicsMatrix[3], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedIntrinsicsMatrix[4], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedIntrinsicsMatrix[5], CultureInfo.InvariantCulture.NumberFormat),
                                              0f));
        projectorInstrinsics.SetRow(2, new Vector4(float.Parse(parsedIntrinsicsMatrix[6], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedIntrinsicsMatrix[7], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedIntrinsicsMatrix[8], CultureInfo.InvariantCulture.NumberFormat),
                                              0f));
        projectorInstrinsics.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
        return projectorInstrinsics;
    }

    float[] ReadProjectorDistortion(XmlDocument xmlDoc)
    {
        float[] projectorDistortion = new float[5];
        XmlNode distortionMatrixNode = xmlDoc.DocumentElement.SelectSingleNode("/opencv_storage/proj_dist/data");
        string[] parsedDistortionCoeficients = GetStringFromXmlNode(distortionMatrixNode);
        for (int i = 0; i < parsedDistortionCoeficients.Length; i++)
        {
            projectorDistortion[i] = float.Parse(parsedDistortionCoeficients[i], CultureInfo.InvariantCulture.NumberFormat);
        }
        return projectorDistortion;
    }

	Matrix4x4 ReadProjectorRotation(XmlDocument xmlDoc)
	{
		Matrix4x4 projectorRotation = new Matrix4x4();
        XmlNode rotationMatrixNode = xmlDoc.DocumentElement.SelectSingleNode("/opencv_storage/rotation/data");
        string[] parsedRotationMatrix = GetStringFromXmlNode(rotationMatrixNode);
        projectorRotation.SetRow(0, new Vector4(float.Parse(parsedRotationMatrix[0], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedRotationMatrix[1], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedRotationMatrix[2], CultureInfo.InvariantCulture.NumberFormat),
                                              0f));
        projectorRotation.SetRow(1, new Vector4(float.Parse(parsedRotationMatrix[3], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedRotationMatrix[4], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedRotationMatrix[5], CultureInfo.InvariantCulture.NumberFormat),
                                              0f));
        projectorRotation.SetRow(2, new Vector4(float.Parse(parsedRotationMatrix[6], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedRotationMatrix[7], CultureInfo.InvariantCulture.NumberFormat),
                                              float.Parse(parsedRotationMatrix[8], CultureInfo.InvariantCulture.NumberFormat),
                                              0f));
        projectorRotation.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
        return projectorRotation;
    }

    Vector3 ReadProjectorPosition(XmlDocument xmlDoc)
    {
        Vector3 projectorPosition = new Vector3();
        XmlNode transVectorNode = xmlDoc.DocumentElement.SelectSingleNode("/opencv_storage/translation/data");
        string[] parsedTransVector = GetStringFromXmlNode(transVectorNode);
        projectorPosition.Set(
           (float.Parse(parsedTransVector[0], CultureInfo.InvariantCulture.NumberFormat) * (-1) / 100),
           (float.Parse(parsedTransVector[1], CultureInfo.InvariantCulture.NumberFormat) * (-1) / 100),
           (float.Parse(parsedTransVector[2], CultureInfo.InvariantCulture.NumberFormat) * (-1) / 100));
        return projectorPosition;
    }

    string[] GetStringFromXmlNode(XmlNode xmlNode)
    {
        string data = xmlNode.InnerText;
        string[] parsedData = data.Trim().Split('\n', ' ');
        parsedData = parsedData.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        return parsedData;
    }
}
