import os
import pygame
import time
import io
import sys
from PIL import Image


def api_color_img():
    image = Image.open(io.BytesIO(response.content))
    return image

pygame.init()
screen = pygame.display.set_mode((0, 0), pygame.FULLSCREEN)
# Set display time per image in seconds
display_time = 0.6

output_dir_path = "capture_"
j = 1

while os.path.exists(f"{output_dir_path}{j}"):
    j += 1
output_dir_path+=str(j)
os.makedirs(output_dir_path)

folder = "graycode_pattern"

i=0
for filename in os.listdir(folder):
    input_file_path = os.path.join(folder, filename)
    image = pygame.image.load(input_file_path)
    image_surface = pygame.Surface.convert_alpha(image)
    image_surface = pygame.transform.scale(image_surface, screen.get_size())
    screen.blit(image_surface, (0, 0))
    pygame.display.flip()
    time.sleep(display_time)

    result = api_color_img()
    if result is None:
        print("Error calling API.")
        exit()
        
    formatted_num = '{:02d}'.format(i)
    output_file_path = os.path.join(output_dir_path, f'graycode_{formatted_num}.jpg')
    result.save(output_file_path)
    i+=1       
    
pygame.quit()
