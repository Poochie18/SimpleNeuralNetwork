using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draw : MonoBehaviour
{
    public LayerMask drawLayer;
    private Sprite drawSprite;
    private Texture2D drawTexture;
    Color[] cleanColor;
    Color32[] curColor;
    public bool Reset_Canvas_On_Play = true;
    public Color Reset_Colour = new Color(1, 1, 1, 0);
    public int brushRadius;

    Vector2 previous_drag_position;

    private void Start()
    {
        drawSprite = this.GetComponent<SpriteRenderer>().sprite;
        drawTexture = drawSprite.texture;

        if (Reset_Canvas_On_Play)
            ResetCanvas();
    }

    public void ResetCanvas()
    {
        cleanColor = new Color[(int)drawSprite.rect.width * (int)drawSprite.rect.height];
        for (int x = 0; x < cleanColor.Length; x++)
            cleanColor[x] = Reset_Colour;
        drawTexture.SetPixels(cleanColor);
        drawTexture.Apply();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePosition, drawLayer.value);
            if(hit != null)
            {
                
                Vector2 pixelPos = WorldToPixelCoordinates(mousePosition);
                curColor = drawTexture.GetPixels32();
                Debug.Log(curColor.Length);
                if (previous_drag_position == Vector2.zero)
                    MarkPixels(pixelPos);
                else
                    ColourBetween(previous_drag_position, pixelPos);
                previous_drag_position = pixelPos;
                //Debug.Log(curColor.Length);
            }
            else if (!Input.GetMouseButton(0))
            {
                previous_drag_position = Vector2.zero;
            }
        }
    }

    public void ColourBetween(Vector2 start_point, Vector2 end_point)
    {
        float distance = Vector2.Distance(start_point, end_point);

        Vector2 cur_position = start_point;

        float lerp_steps = 1 / distance;
        Debug.Log(lerp_steps);
        for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
        {
            cur_position = Vector2.Lerp(start_point, end_point, lerp);
            MarkPixels(cur_position);
        }
    }

    public Vector2 WorldToPixelCoordinates(Vector2 world_position)
    {
        Vector2 local_pos = transform.InverseTransformPoint(world_position);

        float pixelWidth = drawSprite.rect.width;
        float pixelHeight = drawSprite.rect.height;

        float unitsToPixels = pixelWidth / drawSprite.bounds.size.x * transform.localScale.x;

        float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
        float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;

        Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));

        return pixel_pos;
    }

    public void MarkPixels(Vector2 position)
    {
        
        int center_x = (int)position.x;
        int center_y = (int)position.y;

        for (int x = center_x - brushRadius; x <= center_x + brushRadius; x++)
        {
            for (int y = center_y - brushRadius; y <= center_y + brushRadius; y++)
            {
                MarkPixelToChange(x, y);
            }
        }

        
    }

    public void MarkPixelToChange(int x,int y)
    {
        //Debug.Log($"{position.x}:{position.y}");
        int array_pos = y * (int)drawSprite.rect.width + x;
        //Debug.Log(array_pos);
        curColor[array_pos] = new Color(0f, 0f, 0f, 0f);
        drawTexture.SetPixels32(curColor);
        drawTexture.Apply();
    }
}
