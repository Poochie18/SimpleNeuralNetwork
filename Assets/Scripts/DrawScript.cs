using UnityEngine;
using UnityEngine.UI;
using System;
using Color = UnityEngine.Color;

public class DrawScript : MonoBehaviour
{
    public static event Action<double[]> Inputs; 

    private int imageSize = 28;
    private int scale = 16;

    private double[] inputs;

    private Color penColour = Color.white;
    private Color resetColour = Color.black;

    [SerializeField] private int penWidth = 16;

    [SerializeField] private LayerMask drawingLayers;

    [SerializeField] private bool resetOnPlay = true;

    [SerializeField] private Slider slider;

    [SerializeField] private Text sliderValue;

    private Sprite drawSprite;
    private Texture2D drawTexture;

    private Vector2 previousPosition;
    private Color[] cleanColours;
    private Color32[] currentColors;

    private bool mouseWasHeldOut = false;
    private bool NoDraw = false;

    private void Awake()
    {
        drawSprite = GetComponent<SpriteRenderer>().sprite;
        drawTexture = drawSprite.texture;
        SetSliderValues();
        if (resetOnPlay)
            ResetCanvas();
    }

    private void SetSliderValues()
    {
        if(PlayerPrefs.GetInt("FirstOpen", 1) == 1)
        {
            PlayerPrefs.SetInt("FirstOpen", 0);

            slider.value = 6;
            penWidth = 6;
            PlayerPrefs.SetInt("Slider", penWidth);
            sliderValue.text = string.Join("", penWidth);
        }
        else
        {
            slider.value = PlayerPrefs.GetInt("Slider");
            penWidth = PlayerPrefs.GetInt("Slider");

            sliderValue.text = string.Join("", penWidth);
        }
        
    }

    private void SetUpNN(int x, int y)
    {
        int miniX = Mathf.RoundToInt(x / scale);
        int miniY = Mathf.RoundToInt((448 - y) / scale);
        inputs[miniX + miniY * imageSize] = 1f;
        Inputs(inputs);
    }

    private void PenBrush(Vector2 WorldPos)
    {
        Vector2 pixelPos = WorldToPixelCoordinates(WorldPos);

        
        currentColors = drawTexture.GetPixels32();

        if (previousPosition == Vector2.zero)
        {
            MarkPixelsToColour(pixelPos, penWidth, penColour);
        }
        else
        {
            ColourBetween(previousPosition, pixelPos, penWidth, penColour);
        }
        ApplyMarkedPixelChanges();
        previousPosition = pixelPos;
        SetUpNN((int)pixelPos.x, (int)pixelPos.y);
    }
    private void Update()
    {
        bool mouseDown = Input.GetMouseButton(0);
        if (mouseDown && !NoDraw)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos, drawingLayers.value);
            if (hit != null && hit.transform != null)
            {
                PenBrush(mouseWorldPos);
            }

            else
            {
                previousPosition = Vector2.zero;
                if (!mouseWasHeldOut)
                {
                    NoDraw = true;
                }
            }
        }
        else if (!mouseDown)
        {
            previousPosition = Vector2.zero;
            NoDraw = false;
        }
        mouseWasHeldOut = mouseDown;
    }

    private void ColourBetween(Vector2 startPoint, Vector2 endPoint, int width, Color color)
    {
        float distance = Vector2.Distance(startPoint, endPoint);

        Vector2 currentPos = startPoint;

        float lerp_steps = 1 / distance;

        for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
        {
            currentPos = Vector2.Lerp(startPoint, endPoint, lerp);
            MarkPixelsToColour(currentPos, width, color);
        }
    }

    private void MarkPixelsToColour(Vector2 pixelCenter, int thickness, Color color)
    {
        int center_x = (int)pixelCenter.x;
        int center_y = (int)pixelCenter.y;

        for (int x = center_x - thickness; x <= center_x + thickness; x++)
        {
            if (x >= (int)drawSprite.rect.width || x < 0)
                continue;

            for (int y = center_y - thickness; y <= center_y + thickness; y++)
            {
                MarkPixelToChange(x, y, color);
            }
        }
    }
    private void MarkPixelToChange(int x, int y, Color color)
    {
        int arrayPosition = y * (int)drawSprite.rect.width + x;

        if (arrayPosition > currentColors.Length || arrayPosition < 0)
            return;
        
        currentColors[arrayPosition] = color;
        
        
    }
    private void ApplyMarkedPixelChanges()
    {
        drawTexture.SetPixels32(currentColors);
        drawTexture.Apply();
    }


    private Vector2 WorldToPixelCoordinates(Vector2 worldPosition)
    {
        Vector2 local_pos = transform.InverseTransformPoint(worldPosition);

        float pixelWidth = drawSprite.rect.width;
        float pixelHeight = drawSprite.rect.height;

        float unitsToPixels = pixelWidth / drawSprite.bounds.size.x * transform.localScale.x;

        float centered_y = local_pos.y * unitsToPixels + pixelWidth / 2;
        float centered_x = local_pos.x * unitsToPixels + pixelHeight / 2;

        Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));
        return pixel_pos;
    }

    private void ResetCanvas()
    {
        inputs = new double[imageSize * imageSize];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 0f;
        }
        cleanColours = new Color[(int)drawSprite.rect.width * (int)drawSprite.rect.height];
        for (int i = 0; i < cleanColours.Length; i++)
            cleanColours[i] = resetColour;

        drawTexture.SetPixels(cleanColours);
        drawTexture.Apply();
    }

    public void SlideringPedWidth()
    {
        penWidth = (int)slider.value;
        sliderValue.text = string.Join("", (int)slider.value);
        PlayerPrefs.SetInt("Slider", penWidth);
    }

}
