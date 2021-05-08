using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Drawing;
using System.IO;
using UnityEngine.UI;
using Color = System.Drawing.Color;
using Random = System.Random;

public class MainScript : MonoBehaviour
{
    NeuralNetwork nn;
    public Text numbersWeight;
    public Text number;
    private void Start()
    {
        DrawScript.Inputs += TeachNN;
        //nn = new NeuralNetwork();
        Digits();
        Test();


    }
    
    void Test()
    {
        string[] files;
        int sample = 5000;
        string path = @"F:/Download/test";
        files = Directory.GetFiles(path, "*.png");
        int right = 0;
        for (int i = 0; i < sample; i++)
        {
            int imgIndex = i;
            int number = GetTestNumber(imgIndex, files);
            double[] outputs = nn.FeedForward(GetImage(imgIndex, files));
            int num = WhichNumber(outputs);
            if (number == num)
            {
                right++;
            }
        }
        Debug.Log(right);
    }

    private int GetTestNumber(int index, string[] files)
    {
        int number = Convert.ToInt32(files[index].Substring(27, 1));
        return number;
    }

    private void Print(double[] inputs)
    {
        Debug.Log(inputs.Length);
        for (int i = 0; i < inputs.Length / 28; i++)
        {
            for (int j = 0; j < inputs.Length / 28; j++)
            {
                Debug.Log(String.Format("{0,4}", Math.Round(inputs[j + i * 28], 2)));
            }

            Debug.Log("\n");
        }
        Debug.Log("");
    }

    public void Digits()
    {
        string[] files;
        Random rnd = new Random();
        
        nn = new NeuralNetwork(0.01f, 784,256,32, 10);

        int sample = 60000;
        string path = @"F:/Download/train";
        files = Directory.GetFiles(path, "*.png");

        int epochs = 800;
        for (int i = 1; i < epochs; i++)
        {
            int right = 0;
            double error = 0;
            int batchSize = 100;
            for (int j = 0; j < batchSize; j++)
            {
                int imgIndex = (int)(rnd.NextDouble() * sample);
                double[] targets = new double[10] { 0,0,0,0,0,0,0,0,0,0};
                //Debug.Log(targets);
                int number = GetNumber(imgIndex, files);
                targets[number] = 1;
                double[] outputs = nn.FeedForward(GetImage(imgIndex, files));
                int num = WhichNumber(outputs);
                if (number == num)
                {
                    right++;
                }
                for (int k = 0; k < 10; k++)
                {
                    error += (targets[k] - outputs[k]) * (targets[k] - outputs[k]);
                }
                nn.BackPropagation(targets);
            }
            Debug.Log($"Epoch: {i}; Correct: {right}; Error: {error}");
        }
        nn.SaveGame();
    }

    public void TeachNN(double[] inputs)
    {
       
        double[] outputs = nn.FeedForward(inputs);
        //Debug.Log($"{Math.Round(outputs[0], 2)} {Math.Round(outputs[1], 2)} {Math.Round(outputs[2], 2)} {Math.Round(outputs[3], 2)} {Math.Round(outputs[4], 2)} {Math.Round(outputs[5], 2)} {Math.Round(outputs[6], 2)} {Math.Round(outputs[7], 2)} {Math.Round(outputs[8], 2)} {Math.Round(outputs[9], 2)}");
        int num = WhichNumber(outputs);
        numbersWeight.text = string.Join("  ", 
         Math.Round(outputs[0], 2), 
         Math.Round(outputs[1], 2),
         Math.Round(outputs[2], 2),
         Math.Round(outputs[3], 2),
         Math.Round(outputs[4], 2)) + "\n\n" + 
         string.Join("  ", 
         Math.Round(outputs[5], 2),
         Math.Round(outputs[6], 2),
         Math.Round(outputs[7], 2),
         Math.Round(outputs[8], 2),
         Math.Round(outputs[9], 2));
        number.text = string.Join("", num); 
        Debug.Log(num);
    }

    public int WhichNumber(double[] outputs)
    {
        int maxDigit = 0;
        double maxDigitWeight = -1f;
        for (int k = 0; k < 10; k++)
        {
            if (outputs[k] > maxDigitWeight)
            {
                maxDigitWeight = outputs[k];
                maxDigit = k;
            }
        }
        return maxDigit;
    }

    private int GetNumber(int index, string[] files)
    {
        int number = Convert.ToInt32(files[index].Substring(28, 1));
        return number;
    }

    private double[] GetImage(int index, string[] files)
    {
        Color clr;
        double[] inputs = new double[784];

        Bitmap btm = new Bitmap(files[index]);

        for (int i = 0; i < btm.Width; i++)
        {
            for (int j = 0; j < btm.Height; j++)
            {
                clr = btm.GetPixel(j, i);
                inputs[j + i * 28] = (clr.ToArgb() & 0xff) / 255.0f; 
            }
        }
        //Print(inputs);

        return inputs;
    }
}
