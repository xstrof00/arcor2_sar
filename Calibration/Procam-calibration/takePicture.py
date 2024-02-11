import cv2
import time 

def capture_photo(camera_index=-1, photo_filename='captured_photo.jpg'):
    # Inicializace kamery
    cap = cv2.VideoCapture(0)
    time.sleep(0.5)

    # Kontrola, zda je kamera správně inicializována
    if not cap.isOpened():
        print("Chyba: Nelze otevřít kameru.")
        return

    # Snímání fotky
    ret, frame = cap.read()
    
    # Uvolnění kamery
    cap.release()

    # Uložení fotky
    if ret:
        cv2.imwrite(photo_filename, frame)
        print(f"Fotka uložena jako {photo_filename}")
    else:
        print("Chyba při snímání fotky.")

if __name__ == "__main__":
    capture_photo()