import requests
import os
from PIL import Image
import io

systemStartUrl = "http://192.168.104.100:5013/system/start"
systemStartResponse = requests.put(systemStartUrl)
print("Starting system:", systemStartResponse.status_code)

kinectStartUrl = "http://192.168.104.100:5016/state/start"
kinectStartResponse = requests.put(kinectStartUrl)
print("Starting kinect:", kinectStartResponse.status_code)

kinectGetStateUrl = "http://192.168.104.100:5016/state/started"
kinectGetStateResponse = requests.get(kinectGetStateUrl)
print("Is kinect started?:", kinectGetStateResponse.text)

colorImageUrl = "http://192.168.104.100:5016/color/image"
colorImageResponse = requests.get(colorImageUrl)
print("Taking color image:", colorImageResponse.status_code)

kinectStopUrl = "http://192.168.104.100:5016/state/stop"
kinectStartResponse = requests.put(kinectStopUrl)
print("Stopping kinect:", kinectStartResponse.status_code)

systemStopUrl = "http://192.168.104.100:5013/system/stop"
systemStartResponse = requests.put(systemStopUrl)
print("Stopping system:", systemStartResponse.status_code)

