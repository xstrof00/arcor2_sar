using System;
using System.Xml;
using UnityEngine;

namespace Assets.Scripts.CalibrationData
{
    public class SetCalibrationData
    {
        public CalibrationData calibrationData;

        XmlDocument calibrationDataXml = new XmlDocument();
        calibrationDataXml.Load("c:/Users/strof/Programovani/Github/arcor2_sar/Calibration/calibration_result.xml");
        XmlNode translationVectorNode = doc.DocumentElement.SelectSingleNode("/opencv_storage/translation/data");
        string translationVectorData = translationVectorNode.InnerText;
        string[] parsedTranslationVector = translationVectorData.Trim().Split('\n', ' ');
        parsedTranslationVector = parsedTranslationVector.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        calibrationData.projectorTranslation = new Vector3(
            (float.Parse(parsedTransVector[0], CultureInfo.InvariantCulture.NumberFormat)* (-1) / 100),
            (float.Parse(parsedTransVector[1], CultureInfo.InvariantCulture.NumberFormat) * (-1) / 100),
            (float.Parse(parsedTransVector[2], CultureInfo.InvariantCulture.NumberFormat) * (-1) / 100)
        );

        Matrix4x4 rotationMatrix = new Matrix4x4
    }
}
