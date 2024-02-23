using System;
using UnityEngine;

public class CalibrationData
{
	public int projectorHeight { get; set; }
	public int projectorWidth { get; set; }
	public Matrix4x4 cameraIntrinsics { get; set; }
	public Matrix4x4 cameraDistortion { get; set; }
    public Matrix4x4 projectorIntrinsics { get; set; }
    public Matrix4x4 projectorDistortion { get; set; }
	public Matrix4x4 projectorRotation { get; set; }
	public Vector3 projectorTranslation { get; set; }
}
