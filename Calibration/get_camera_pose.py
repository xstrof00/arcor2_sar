# author: Jakub Štrof

import time
import requests
import io
import cv2
from PIL import Image
import xml.etree.ElementTree as ET

class CamParameters():
    inverse: bool = False
    cx: float = 0.0
    cy: float = 0.0
    fx: float = 0.0
    fy: float = 0.0
    distortionCoeffs: list = [0.0, 0.0, 0.0, 0.0, 0.0]


class Position():
    x: float = 0.0
    y: float = 0.0
    z: float = 0.0

class Orientation():
    w: float = 1.0
    x: float = 0.0
    y: float = 0.0
    z: float = 0.0
    

kinectPosition = Position()
kinectOrientation = Orientation()

kinectPose = {
    'position': {
        'x': kinectPosition.x,
        'y': kinectPosition.y,
        'z': kinectPosition.z
    },
    'orientation': {
        'x': kinectOrientation.x,
        'y': kinectOrientation.y,
        'z': kinectOrientation.z,
        'w': kinectOrientation.w
    }
}

def fullStart():
    kinectStartUrl = "http://192.168.104.100:5017/state/full-start"
    kinectStartResponse = requests.put(kinectStartUrl, json=kinectPose)
    print("Starting kinect:", kinectStartResponse.status_code)

def getState():
    kinectGetStateUrl = "http://192.168.104.100:5017/state/started"
    kinectGetStateResponse = requests.get(kinectGetStateUrl)
    print("Is kinect started?:", kinectGetStateResponse.text)

def getColorImage():
    colorImageUrl = "http://192.168.104.100:5017/color/image"
    colorImageResponse = requests.get(colorImageUrl)
    print("Taking color image:", colorImageResponse.status_code)
    return colorImageResponse

def stopKinect():
    kinectStopUrl = "http://192.168.104.100:5017/state/stop"
    kinectStartResponse = requests.put(kinectStopUrl)
    print("Stopping kinect:", kinectStartResponse.status_code)

def saveCalibrationImage(colorImageResponse):
    kinectImage = Image.open(io.BytesIO(colorImageResponse.content))
    kinectImage.save('get_camera_pose_photo.png', format='PNG')

def calibrateKinect(colorImageResponse, jsonCamParameters):
    kinectCalibrateUrl = "http://192.168.104.100:5014/calibrate/camera"
    kinectCalibrateResponse = requests.put(kinectCalibrateUrl, files={'image': colorImageResponse.content}, params=jsonCamParameters)
    print("Calibrating kinect:", kinectCalibrateResponse.text)
    return kinectCalibrateResponse

#Conneting to ARServer via ssh and taking a picture with kinect directly, gettting its pose with calibration API

def takePicture():
    cap = cv2.VideoCapture(-1)
    time.sleep(0.5)

    if not cap.isOpened():
        print("Error: Cannot open the camera.")
        return

    ret, frame = cap.read()

    cap.release()
    photo_filename='get_camera_pose_photo.png'

    if ret:
        cv2.imwrite(photo_filename, frame)
        print(f"Image saved as {photo_filename}")
        return frame
    else:
        print("Error occured while taking the picture.")

def getCamIntrinsics():
    xmlTree = ET.parse('../Assets/Resources/calibration_result.xml')
    root = xmlTree.getroot()
    camInt = root.find('cam_int').findtext('data')
    camIntString = str(camInt)
    return camIntString

def parseCamIntMatrix(camIntMatrix):
    numbers = []
    lines = camIntMatrix.strip().split('\n')
    for line in lines:
        values = line.strip().split(' ')
        for value in values:
            numbers.append(value)
    return numbers

def getCamDistCoefficients():
    xmlTree = ET.parse('calibration_result.xml')
    root = xmlTree.getroot()
    camDistCoeffs = root.find('cam_dist').findtext('data')
    camDistCoeffsString = str(camDistCoeffs)
    return camDistCoeffsString

if __name__ == "__main__":
    camIntrinsicsMatrix = getCamIntrinsics()
    camIntrinsicsArray = parseCamIntMatrix(camIntrinsicsMatrix)
    camDistCoefficients = getCamDistCoefficients()

    camParameters = CamParameters()
    camParameters.cx = camIntrinsicsArray[2]
    camParameters.cy = camIntrinsicsArray[5]
    camParameters.fx = camIntrinsicsArray[0]
    camParameters.fy = camIntrinsicsArray[4]
    camParameters.distortionCoeffs = camDistCoefficients

    jsonCamParameters = camParameters.__dict__
    
    fullStart()
    getState()
    colorImageResponse = getColorImage()
    stopKinect()
    saveCalibrationImage(colorImageResponse)
    kinectCalibrateResponse = calibrateKinect(colorImageResponse, jsonCamParameters)
    

    kinectCalibrationFile = "../Assets/Resources/kinectCalibrationData.json"
    with open(kinectCalibrationFile, 'w') as file:
        file.write(kinectCalibrateResponse.text)