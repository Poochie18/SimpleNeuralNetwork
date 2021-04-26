using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Drawing;

public class Draw : MonoBehaviour
{
    private int width = 28;
    private int height = 28;
    private int scale = 16;

    private bool mousePressed = false;
    private int mx = 0;
    private int my = 0;

    private double[,] colors;
    double[] inputs;

    private Bitmap img;
    private Bitmap miniImg;

    private NeuralNetwork nn;

    private void Start()
    {
        colors = new double[width, height];
        img = new Bitmap(@"F:/Repository/SimpleNeuralNetwork/Assets/Resources/sprt.png");
        miniImg = new Bitmap(width, height);
        inputs = new double[width * height];
    }

    private void Update()
    {
        mousePressed = Input.GetMouseButton(0);
        if (mousePressed)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //
            var coordinates = WorldToPixelCoordinates(mouseWorldPos);
            Debug.Log($"{(int)coordinates.x},{(int)coordinates.y}");
           
            
            img.SetPixel((int)coordinates.x, (int)coordinates.y, System.Drawing.Color.Red);
            Debug.Log(img.GetPixel((int)coordinates.x, (int)coordinates.y));
        }
    }

    public Vector2 WorldToPixelCoordinates(Vector2 worldPosition)
    {
        Vector2 local_pos = transform.InverseTransformPoint(worldPosition);

        float pixelWidth = img.Width;
        float pixelHeight = img.Height;

        float unitsToPixels = pixelWidth / (pixelWidth / 100f) * transform.localScale.x;

        float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
        float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;

        Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));
        //Debug.Log(pixel_pos);
        return pixel_pos;
    }

    public void Paint()
    {
        
        
    }
}
