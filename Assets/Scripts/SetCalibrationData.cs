using System.Globalization;
using System.Xml;
using UnityEngine;
using System.Linq;

public class SetCalibrationData
{
    public CalibrationData calibrationData;

    void GetCalibrationData()
    {
        XmlDocument doc = new XmlDocument();
        doc.Load("c:/Users/strof/Programovani/Github/arcor2_sar/AR_test_2/calibration_result.xml");
        XmlNode translationVectorNode = doc.DocumentElement.SelectSingleNode("/opencv_storage/translation/data");
        string translationVectorData = translationVectorNode.InnerText;
        string[] parsedTranslationVector = translationVectorData.Trim().Split('\n', ' ');
        parsedTranslationVector = parsedTranslationVector.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        calibrationData.projectorTranslation = new Vector3(
            (float.Parse(parsedTranslationVector[0], CultureInfo.InvariantCulture.NumberFormat)* (-1) / 100),
            (float.Parse(parsedTranslationVector[1], CultureInfo.InvariantCulture.NumberFormat) * (-1) / 100),
            (float.Parse(parsedTranslationVector[2], CultureInfo.InvariantCulture.NumberFormat) * (-1) / 100)
        );
    }



}
