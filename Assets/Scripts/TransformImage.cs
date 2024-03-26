using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetImagePosition : MonoBehaviour
{
    public Image image;
    public Canvas canvas;
    public GameObject projector;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        double projectorWidth = canvas.pixelRect.width;
        double projectorHeight = canvas.pixelRect.height;
        double projectionWidthInCm = 112.5;
        double projectionHeightInCm = 64;
        double cmInPixelsWidth = projectorWidth / projectionWidthInCm;
        double cmInPixelsHeight = projectorHeight / projectionHeightInCm;

        double projectorYInPixels = (projector.transform.position.y * (-1) * 10) * cmInPixelsHeight;
        double projectorXInPixels = (projector.transform.position.x * (-1) * 10) * cmInPixelsWidth;

        

        Vector2 setImagePosition = new Vector2((float)projectorXInPixels, (float)projectorYInPixels);
        //Debug.Log(setImagePosition);
        //image.rectTransform.position = setImagePosition;
    }

}
