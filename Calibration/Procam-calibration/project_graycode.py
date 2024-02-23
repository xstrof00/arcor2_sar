# import the  pygame module
import pygame
import time
import os
import cv2
os.environ['SDL_AUDIODRIVER'] = 'dummy'

output_dir_path = "capture_"

def takePicture(i):
    cap = cv2.VideoCapture(0)
    time.sleep(0.5)

    if not cap.isOpened():
        print("Chyba: Nelze otevřít kameru.")
        return

    ret, frame = cap.read()

    cap.release()

    if ret:
        return frame
    else:
        print("Chyba při snímání fotky.")

input_folder = "graycode_pattern"

pygame.init()

j = 1

while os.path.exists(f"{output_dir_path}{j}"):
    j += 1
    
output_dir_path+=str(j)
os.makedirs(output_dir_path)

# assigning values to displayHeight and displayWidth
displayHeight = 800
displayWidth = 1280

display_surface = pygame.display.set_mode((displayWidth, displayHeight), pygame.FULLSCREEN, 0, display=1)

i=0

for filename in sorted(os.listdir(input_folder)):
    input_file_path = os.path.join(input_folder, filename)
    image = pygame.image.load(input_file_path)
    image_surface = pygame.Surface.convert_alpha(image)
    image_surface = pygame.transform.scale(image_surface, display_surface.get_size())
    display_surface.blit(image_surface, (0, 0))
    pygame.display.flip()
    time.sleep(0.1)

    result = takePicture(i)
    formatted_num = '{:02d}'.format(i)
    output_file_path = os.path.join(output_dir_path, f'graycode_{formatted_num}.jpg')
    cv2.imwrite(output_file_path, result)
    i+=1       
    
pygame.quit()
