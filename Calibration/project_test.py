# import the  pygame module
import pygame
import sys

# initialising Pyame library.
pygame.init()

# assigning values to displayHeight and displayWidth
displayHeight = 1080
displayWidth = 1920

displaySurface = pygame.display.set_mode((displayWidth, displayHeight ))

# set the pygame window name
pygame.display.set_caption('Coding Ninjas Logo')

# create a surface object, image is drawn on it.
image = pygame.image.load(r'graycode_pattern/pattern_08.png')

# infinite loop
while True:
      #  fill the surface object with white colour
    displaySurface.fill((255, 255, 255))

      # to display surface object at (0, 0) coordinate.
    displaySurface.blit(image, (0, 0))
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            sys.exit()

    pygame.display.update()