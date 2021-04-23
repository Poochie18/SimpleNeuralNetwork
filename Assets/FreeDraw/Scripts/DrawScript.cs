﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawScript : MonoBehaviour
{
    private Color penColour = Color.white;
    private Color resetColour = Color.black;

    public int penWidth = 3;

    public LayerMask drawingLayers;

    public bool resetOnPlay = true;

    private Sprite drawSprite;
    private Texture2D drawTexture;

    private Vector2 previousPosition;
    private Color[] cleanColours;
    private Color32[] currentColors;
    bool mouseWasHeldOut = false;
    bool NoDraw = false;

    void Awake()
    {
        drawSprite = this.GetComponent<SpriteRenderer>().sprite;
        drawTexture = drawSprite.texture;

        if (resetOnPlay)
            ResetCanvas();
    }

    public void PenBrush(Vector2 WorldPos)
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
    }
    void Update()
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

    public void ColourBetween(Vector2 startPoint, Vector2 endPoint, int width, Color color)
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

    public void MarkPixelsToColour(Vector2 pixelCenter, int thickness, Color color)
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
    public void MarkPixelToChange(int x, int y, Color color)
    {
        int arrayPosition = y * (int)drawSprite.rect.width + x;

        if (arrayPosition > currentColors.Length || arrayPosition < 0)
            return;

        currentColors[arrayPosition] = color;
    }
    public void ApplyMarkedPixelChanges()
    {
        drawTexture.SetPixels32(currentColors);
        drawTexture.Apply();
    }


    public Vector2 WorldToPixelCoordinates(Vector2 worldPosition)
    {
        Vector2 local_pos = transform.InverseTransformPoint(worldPosition);

        float pixelWidth = drawSprite.rect.width;
        float pixelHeight = drawSprite.rect.height;

        float unitsToPixels = pixelWidth / drawSprite.bounds.size.x * transform.localScale.x;

        float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
        float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;

        Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));

        return pixel_pos;
    }

    public void ResetCanvas()
    {
        cleanColours = new Color[(int)drawSprite.rect.width * (int)drawSprite.rect.height];
        for (int i = 0; i < cleanColours.Length; i++)
            cleanColours[i] = resetColour;

        drawTexture.SetPixels(cleanColours);
        drawTexture.Apply();
    }

}