import requests
import io
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
    
position = Position()
orientation = Orientation()

data = {
    'position': {
        'x': position.x,
        'y': position.y,
        'z': position.z
    },
    'orientation': {
        'x': orientation.x,
        'y': orientation.y,
        'z': orientation.z,
        'w': orientation.w
    }
}

kinectStartUrl = "http://192.168.104.100:5017/state/full-start"
kinectStartResponse = requests.put(kinectStartUrl, json=data)
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

kinectImage = Image.open(io.BytesIO(colorImageResponse.content))
kinectImage.save('kinect_image.png') 

#TODO - Provolat endpoint pro kalibraci kinectu a zíkání pózy (přes kalibrační službu na jiném portu nebo přes RPC na ARServeru)

#kinectCalibrateUrl = "http://192.168.104.100:5014/calibrate/camera"
#kinectCalibrateResponse = requests.put(kinectCalibrateUrl)
#print("Calibrating kinect:", kinectCalibrateResponse.status_code)
