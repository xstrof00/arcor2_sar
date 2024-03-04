import cv2
import time 

def capturePhoto():
    cap = cv2.VideoCapture(-1)
    time.sleep(0.5)

    if not cap.isOpened():
        print("Error: Cannot open the camera.")
        return
    
    ret, frame = cap.read()
    cap.release()

    photo_filename='take_picutre_photo.jpg'

    if ret:
        cv2.imwrite(photo_filename, frame)
        print(f"Image saved as {photo_filename}")
    else:
        print("Error occured while taking the picture.")

if __name__ == "__main__":
    capturePhoto()