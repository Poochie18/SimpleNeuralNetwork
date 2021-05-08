using System;
using UnityEngine;
using System.Drawing;
using System.IO;
using UnityEngine.UI;

using Color = System.Drawing.Color;
using Random = System.Random;

public class MainScript : MonoBehaviour
{
    NeuralNetwork nn; //NeuralNetwork object

    [SerializeField] private Text numbersWeightLeft;
    [SerializeField] private Text numbersWeightRight;

    [SerializeField] private Text number; 

    [SerializeField] private int sample = 60000; //Number of Images for treaning
    [SerializeField] private int sampleTest = 60000; //Number of Images for test
    [SerializeField] private int epochs = 1000; //Number of Epochs

    private const int numbersCount = 10; //number of output neurons
    private int imageSize = 28; // the size of the picture is 28 because it is square

    private void Start()
    {
        DrawScript.Inputs += CheckingNN;
        nn = new NeuralNetwork();
        //Digits();
        //Test();
    }
    
    private void TestingNN()
    {
        string[] files;
        string path = @"F:/Download/test";
        files = Directory.GetFiles(path, "*.png");
        int correctResult = 0;
        for (int imgIndex = 0; imgIndex < sampleTest; imgIndex++)
        {
            int targetNumber = GetNumber(imgIndex, files, 27);
            double[] outputs = nn.FeedForward(GetImage(imgIndex, files));
            int estimatedNum = WhichNumber(outputs);
            if (targetNumber == estimatedNum)
            {
                correctResult++;
            }
        }
        Debug.Log(correctResult);
    }

    private int GetNumber(int index, string[] files, int size)
    {
        return Convert.ToInt32(files[index].Substring(size, 1));
    }

    private void Digits()
    {
        Random rnd = new Random();
        
        nn = new NeuralNetwork(0.001f, 784,256, 10); //Creating a neural network object

        //Loading files into an array
        string[] files;
        string path = @"F:/Download/train";
        files = Directory.GetFiles(path, "*.png");


        for (int i = 1; i < epochs; i++)
        {
            int correctResult = 0;
            double error = 0;
            int batchSize = 100; //Number of packs of pictures
            for (int j = 0; j < batchSize; j++)
            {
                int imgIndex = (int)(rnd.NextDouble() * sample); //Random Image

                //Get target number
                double[] targets = new double[numbersCount] { 0,0,0,0,0,0,0,0,0,0}; 
                int targetNumber = GetNumber(imgIndex, files, 28);
                targets[targetNumber] = 1;

                //Run the picture through the neural network and get an array of 10 numbers at the output
                double[] outputs = nn.FeedForward(GetImage(imgIndex, files));

                //Determine which number has the highest value and compare with the target number
                int estimatedNum = WhichNumber(outputs);
                if (targetNumber == estimatedNum)
                {
                    correctResult++;
                }

                // Calculate error (Target[i] - Output[i])^2
                for (int k = 0; k < numbersCount; k++)
                {
                    error += (targets[k] - outputs[k]) * (targets[k] - outputs[k]);
                }

                //Backpropagate with an error
                nn.BackPropagation(targets);
            }
            Debug.Log($"Epoch: {i}; Correct: {correctResult}; Error: {error}");
        }
        //nn.SaveGame();
    }

    public void CheckingNN(double[] inputs)
    {
        double[] outputs = nn.FeedForward(inputs);
        //Debug.Log($"{Math.Round(outputs[0], 2)} {Math.Round(outputs[1], 2)} {Math.Round(outputs[2], 2)} {Math.Round(outputs[3], 2)} {Math.Round(outputs[4], 2)} {Math.Round(outputs[5], 2)} {Math.Round(outputs[6], 2)} {Math.Round(outputs[7], 2)} {Math.Round(outputs[8], 2)} {Math.Round(outputs[9], 2)}");
        int num = WhichNumber(outputs);
        numbersWeightLeft.text = string.Join("",
         Math.Round(outputs[0], 3)+ "\n" +
         Math.Round(outputs[1], 3) + "\n" +
         Math.Round(outputs[2], 3) + "\n" +
         Math.Round(outputs[3], 3) + "\n" +
         Math.Round(outputs[4], 3));
        numbersWeightRight.text = string.Join("",
         Math.Round(outputs[5], 3) + "\n" +
         Math.Round(outputs[6], 3) + "\n" +
         Math.Round(outputs[7], 3) + "\n" +
         Math.Round(outputs[8], 3) + "\n" +
         Math.Round(outputs[9], 3));
        number.text = string.Join("", num); 
        //Debug.Log(num);
    }

    public int WhichNumber(double[] outputs)
    {
        int maxDigit = 0;
        double maxDigitWeight = -1f;
        for (int k = 0; k < numbersCount; k++)
        {
            if (outputs[k] > maxDigitWeight)
            {
                maxDigitWeight = outputs[k];
                maxDigit = k;
            }
        }
        return maxDigit;
    }

    private double[] GetImage(int index, string[] files)
    {
        Color clr;
        double[] inputs = new double[imageSize* imageSize];

        Bitmap btm = new Bitmap(files[index]);

        for (int i = 0; i < btm.Width; i++)
        {
            for (int j = 0; j < btm.Height; j++)
            {
                clr = btm.GetPixel(j, i);
                inputs[j + i * imageSize] = (clr.ToArgb() & 0xff) / 255.0f; 
            }
        }
        return inputs;
    }
}
