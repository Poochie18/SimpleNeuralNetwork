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
    private Layer[] layers;
    private Random rnd = new Random();
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

    public NeuralNetwork()
    {
        LoadGame();
    }

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

    public void BackPropagation(double[] targets)
    {
        double[] errors = new double[layers[layers.Length - 1].size];
        for (int i = 0; i < layers[layers.Length - 1].size; i++)
        {
            errors[i] = targets[i] - layers[layers.Length - 1].neurons[i];
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
        FileStream file = File.Create(Application.persistentDataPath + "/SavedData.dat");
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
        Print(layers[1].biases);
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Game Saved");
    }

    void Print(double[] b)
    {
        for(int i = 0; i < b.Length; i++)
        {
            Debug.Log(b[i]);
        }
    }

    public void LoadGame()
    {
        if(File.Exists(Application.persistentDataPath + "/SavedData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/SavedData.dat", FileMode.Open);
            SaveData[] data = (SaveData[])bf.Deserialize(file);
            file.Close();

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
            Print(layers[1].biases);
            Debug.Log("Game Loaded");
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