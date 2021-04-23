using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Drawing;
using System.IO;
using Color = System.Drawing.Color;
using Random = System.Random;

public class MainScript : MonoBehaviour
{

    private string[] files;

    private void Start()
    {
        Digits();
    }

    private void Print(double[] inputs)
    {
        Console.WriteLine(inputs.Length);
        for (int i = 0; i < inputs.Length / 28; i++)
        {
            for (int j = 0; j < inputs.Length / 28; j++)
            {
                Console.Write(String.Format("{0,4}", Math.Round(inputs[j + i * 28], 2)));
            }

            Console.WriteLine();
        }
        Console.WriteLine();
    }

    public void Digits()
    {
        Random rnd = new Random();
        NeuralNetwork nn = new NeuralNetwork(0.001f, 784, 32, 16, 10);

        int sample = 600;
        string path = @"F:/Download/train";
        files = Directory.GetFiles(path, "*.png");

        int epochs = 100;
        for (int i = 1; i < epochs; i++)
        {
            int right = 0;
            double error = 0;
            int batchSize = 100;
            for (int j = 0; j < batchSize; j++)
            {
                int imgIndex = (int)(rnd.NextDouble() * sample);
                double[] targets = new double[10];
                int number = GetNumber(imgIndex);
                targets[number] = 1;
                double[] outputs = nn.FeedForward(GetImage(imgIndex));
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
                if (number == maxDigit)
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
        /*Debug.Log(inputs.Length);
        for (int k = 0; k < sample; k++)
        {
            for (int i = 0; i < btm[0].Width; i++)
            {
                for (int j = 0; j < btm[0].Height; j++)
                    Debug.Log(String.Format("{0,3}", inputs[k, i + j * 28]));
                Debug.Log(" ");
            }
        }*/

    }

    private int GetNumber(int index)
    {
        int number = Convert.ToInt32(files[index].Substring(28, 1));
        return number;
    }

    private double[] GetImage(int index)
    {
        Color clr;
        double[] inputs = new double[784];

        Bitmap btm = new Bitmap(files[index]);

        for (int i = 0; i < btm.Width; i++)
        {
            for (int j = 0; j < btm.Height; j++)
            {
                clr = btm.GetPixel(i, j);
                inputs[i + j * 28] = (clr.ToArgb() & 0xff) / 255.0f;
            }
        }
        //Print(inputs);

        return inputs;
    }
}
