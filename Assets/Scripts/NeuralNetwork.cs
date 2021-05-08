using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class NeuralNetwork
{
    private double learningRate;
    private Layer[] layers; // Number of layers
    private Random rnd = new Random();

    //Constructor for the first training
    public NeuralNetwork(double learningRate, params int[] sizes)
    {
        this.learningRate = learningRate;
        layers = new Layer[sizes.Length];
        for (int i = 0; i < sizes.Length; i++)
        {
            int nextSize = 0;
            if (i < sizes.Length - 1) nextSize = sizes[i + 1];
            layers[i] = new Layer(sizes[i], nextSize);
            for (int j = 0; j < sizes[i]; j++)
            {
                layers[i].biases[j] = rnd.Next(-10000, 10000) / 10000f;
                for (int k = 0; k < nextSize; k++)
                {
                    layers[i].weights[j, k] = rnd.Next(-10000, 10000) / 10000f;
                }
            }
        }
    }

    //Constructor for a trained neural network
    public NeuralNetwork()
    {
        LoadGame();
    }

    //Getting output values
    public double[] FeedForward(double[] inputs)
    {

        System.Array.Copy(inputs, 0, layers[0].neurons, 0, inputs.Length);
        for (int i = 1; i < layers.Length; i++)
        {
            Layer l = layers[i - 1];
            Layer l1 = layers[i];
            for (int j = 0; j < l1.size; j++)
            {
                l1.neurons[j] = 0;

                for (int k = 0; k < l.size; k++)
                {
                    l1.neurons[j] += l.neurons[k] * l.weights[k, j];
                }
                l1.neurons[j] += l1.biases[j];
                l1.neurons[j] = Sigmoid(l1.neurons[j]);

            }
        }

        return layers[layers.Length - 1].neurons;
    }


    private double Sigmoid(double x)
    {
        return 1 / (1 + Mathf.Exp(-(float)x));
    }

    private double Dsigmoid(double x)
    {
        return x * (1 - x);
    }

    //Backpropagation method to increase accuracy and reduce guessing error
    public void BackPropagation(double[] targets)
    {
        double[] errors = new double[layers[layers.Length - 1].size];
        for (int i = 0; i < layers[layers.Length - 1].size; i++)
        {
            errors[i] = (targets[i]-layers[layers.Length - 1].neurons[i]);
        }
        for (int k = layers.Length - 2; k >= 0; k--)
        {
            Layer l = layers[k];
            Layer l1 = layers[k + 1];

            double[] errorsNext = new double[l.size];
            double[] gradients = new double[l1.size];
            for (int i = 0; i < l1.size; i++)
            {
                gradients[i] = errors[i] * Dsigmoid(layers[k + 1].neurons[i]);
                gradients[i] *= learningRate;
            }
            double[,] deltas = new double[l1.size, l.size];
            for (int i = 0; i < l1.size; i++)
            {
                for (int j = 0; j < l.size; j++)
                {
                    deltas[i, j] = gradients[i] * l.neurons[j];
                }
            }
            for (int i = 0; i < l.size; i++)
            {
                errorsNext[i] = 0;
                for (int j = 0; j < l1.size; j++)
                {
                    errorsNext[i] += l.weights[i, j] * errors[j];
                }
            }
            errors = new double[l.size];
            System.Array.Copy(errorsNext, 0, errors, 0, l.size);
            double[,] weightsNew = new double[l.weights.GetLength(0), l.weights.GetLength(1)];
            for (int i = 0; i < l1.size; i++)
            {
                for (int j = 0; j < l.size; j++)
                {
                    weightsNew[j, i] = l.weights[j, i] + deltas[i, j];
                }
            }
            l.weights = weightsNew;
            for (int i = 0; i < l1.size; i++)
            {
                l1.biases[i] += gradients[i];
            }
        }
    }



    public void SaveGame()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.dataPath + "/StreamingAssets/SavedData2.dat");
        SaveData[] data = new SaveData[layers.Length];

        for (int i = 0; i < data.Length; i++)
        {
            int nextSize = 0;
            if (i < layers.Length - 1) nextSize = layers[i + 1].size;
            data[i] = new SaveData(layers[i].size, nextSize);
            for (int j = 0; j < layers[i].size; j++)
            {
                data[i].biases[j] = layers[i].biases[j];
                for (int k = 0; k < nextSize; k++)
                {
                    data[i].weights[j, k] = layers[i].weights[j, k];
                }
            }
        }
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Game Saved");
    }

    private void LoadGame()
    {
        string fileName = "SavedData.dat";

        if (!File.Exists(Application.persistentDataPath + "/" + fileName)) UnpackMobileFile(fileName);


        if (File.Exists(Application.persistentDataPath + "/" + fileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fileOpen = File.Open(Application.persistentDataPath + "/" + fileName, FileMode.Open);
            SaveData[] data = (SaveData[])bf.Deserialize(fileOpen);
            fileOpen.Close();

            layers = new Layer[data.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                int nextSize = 0;
                if (i < data.Length - 1) nextSize = data[i + 1].size;
                layers[i] = new Layer(data[i].size, nextSize);
                for (int j = 0; j < data[i].size; j++)
                {
                    layers[i].biases[j] = data[i].biases[j];
                    for (int k = 0; k < nextSize; k++)
                    {
                        layers[i].weights[j, k] = data[i].weights[j, k];
                    }
                }
            }
            Debug.Log("Game Loaded");
        }
    }
    private void UnpackMobileFile(string fileName)
    {  
        string destinationPath = Path.Combine(Application.persistentDataPath, fileName);
        string sourcePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (!File.Exists(destinationPath) || (File.GetLastWriteTimeUtc(sourcePath) > File.GetLastWriteTimeUtc(destinationPath)))
        {
            if (sourcePath.Contains("://"))
            {
                WWW www = new WWW(sourcePath);
                while (!www.isDone) {; }
                if (String.IsNullOrEmpty(www.error))
                {
                    File.WriteAllBytes(destinationPath, www.bytes);
                }
                else
                {
                    Debug.Log("ERROR: the file DB named " + fileName + " doesn't exist in the StreamingAssets Folder, please copy it there.");
                }
            }
            else
            { 
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destinationPath, true);
                }
                else
                {
                    Debug.Log("ERROR: the file DB named " + fileName + " doesn't exist in the StreamingAssets Folder, please copy it there.");
                }
            }
        }
    }
}

[Serializable]
public class SaveData
{
    public double[] biases;
    public double[,] weights;
    public int size;
    public SaveData(int size, int nextSize)
    {
        this.size = size;
        biases = new double[size];
        weights = new double[size, nextSize];
    }
}

public class Layer
{
    public int size;
    public double[] neurons;
    public double[] biases;
    public double[,] weights;

    public Layer(int size, int nextSize)
    {
        this.size = size;
        neurons = new double[size];
        biases = new double[size];
        weights = new double[size,nextSize];
    }
}