import numpy as np
import pygame
import time
import os
import cv2
import requests
import io
from PIL import Image
from get_camera_pose import Position, Orientation


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

outputDirPath = "capture_"

def takePicture():
    cap = cv2.VideoCapture(-1)
    time.sleep(0.1)

    if not cap.isOpened():
        print("Error: Cannot open the camera.")
        return

    ret, frame = cap.read()

    cap.release()

    if ret:
        return frame
    else:
        print("Error occured while taking the picture.")


def takePictureViaAPI():
    colorImageUrl = "http://192.168.104.100:5017/color/image"
    colorImageResponse = requests.get(colorImageUrl)
    return colorImageResponse

def startKinect():
    kinectStartUrl = "http://192.168.104.100:5017/state/full-start"
    kinectStartResponse = requests.put(kinectStartUrl, json=kinectPose)
    print("Starting kinect:", kinectStartResponse.status_code)

def stopKinect():
    kinectStopUrl = "http://192.168.104.100:5017/state/stop"
    kinectStartResponse = requests.put(kinectStopUrl)
    print("Stopping kinect:", kinectStartResponse.status_code)


inputFolder = "graycode_pattern"

pygame.init()

j = 1

while os.path.exists(f"{outputDirPath}{j}"):
    j += 1
    
outputDirPath+=str(j)
os.makedirs(outputDirPath)

displayHeight = 1080
displayWidth = 1920

displaySurface = pygame.display.set_mode((displayWidth, displayHeight), pygame.FULLSCREEN, 0, display=1)

i=0
startKinect()
for filename in sorted(os.listdir(inputFolder)):
    inputFilePath = os.path.join(inputFolder, filename)
    image = pygame.image.load(inputFilePath)
    imageSurface = pygame.Surface.convert_alpha(image)
    imageSurface = pygame.transform.scale(imageSurface, displaySurface.get_size())
    displaySurface.blit(imageSurface, (0, 0))
    pygame.display.flip()
    time.sleep(0.2)

    result = takePictureViaAPI()
    
    kinectImage = Image.open(io.BytesIO(result.content))
    kinectImage = np.array(kinectImage)
    kinectImage = cv2.cvtColor(kinectImage, cv2.COLOR_RGB2BGR)
    
    formattedNum = '{:02d}'.format(i)
    outputFilePath = os.path.join(outputDirPath, f'graycode_{formattedNum}.png')
    cv2.imwrite(outputFilePath, kinectImage)
    i+=1  
stopKinect()     
    
pygame.quit()
