import time
import requests
import io
import cv2
from PIL import Image

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

cameraParameters = {
    'inverse': False,
    'fx': 626.6809,
    'fy': 629.0327,
    'cx': 627.1297,
    'cy': 352.9711,
    'distCoefs': [0.097203, 0.1358340, -0.00830756, -0.00814534, -0.5983313]
}

#Using API to take picture via Kinect and get its pose

'''kinectStartUrl = "http://192.168.104.100:5017/state/full-start"
kinectStartResponse = requests.put(kinectStartUrl, json=kinectPose)
print("Starting kinect:", kinectStartResponse.status_code)

kinectGetStateUrl = "http://192.168.104.100:5017/state/started"
kinectGetStateResponse = requests.get(kinectGetStateUrl)
print("Is kinect started?:", kinectGetStateResponse.text)

colorImageUrl = "http://192.168.104.100:5017/color/image"
colorImageResponse = requests.get(colorImageUrl)
print("Taking color image:", colorImageResponse.status_code)

kinectStopUrl = "http://192.168.104.100:5017/state/stop"
kinectStartResponse = requests.put(kinectStopUrl)
print("Stopping kinect:", kinectStartResponse.status_code)

#with open('kinect_image.bin', 'wb') as file:
#    file.write(colorImageResponse.content)

kinectImage = Image.open(io.BytesIO(colorImageResponse.content))
kinectImage.save('kinect_image2.png', format='PNG')

    kinectCalibrateUrl = "http://192.168.104.100:5014/calibrate/camera"
    kinectCalibrateResponse = requests.put(kinectCalibrateUrl, files={'image': colorImageResponse.content}, params=cameraParameters)
    print("Calibrating kinect:", kinectCalibrateResponse.text)'''

#Conneting to ARServer via ssh and taking a picture with kinect directly, gettting its pose with calibration API

def takePicture():
    cap = cv2.VideoCapture(-1)
    time.sleep(0.5)

    if not cap.isOpened():
        print("Error: Cannot open the camera.")
        return

    ret, frame = cap.read()

    cap.release()
    photo_filename='get_camera_pose_photo.jpg'

    if ret:
        cv2.imwrite(photo_filename, frame)
        print(f"Image saved as {photo_filename}")
        return frame
    else:
        print("Error occured while taking the picture.")

if __name__ == "__main__":
    kinectPicture = takePicture()
    kinectPictureImage = Image.fromarray(kinectPicture)
    kinectPictureBytes = io.BytesIO()
    kinectPictureImage.save(kinectPictureBytes, format='JPEG')
    binaryPicture = kinectPictureBytes.getvalue()

    kinectCalibrateUrl = "http://192.168.104.100:5014/calibrate/camera"
    kinectCalibrateResponse = requests.put(kinectCalibrateUrl, files={'image': binaryPicture}, params=cameraParameters)
    print("Calibrating kinect:", kinectCalibrateResponse.text)