using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    [Header("Layers Settings")]
    public InputLayer inputLayer;
    public HiddenLayer[] hiddenLayers;
    public OutputLayer outputLayer;

    [Header("Visualization Settings")]
    public bool drawLayers = true;
    [Range(2f, 10f)]
    public float neuronsHorizontalPadding = 2f;

    [Header("Training Settings")]
    public float LearningRate = 0.5f;

    [Header("Testing Settings")]
    public DataContainer InputData;

    private ILayer[] layers;

    void Awake()
    {
        SetupLayers();

        if(drawLayers)
            DrawLayers();
    }

    private void SetupLayers()
    {
        layers = new ILayer[1 + hiddenLayers.Length + 1];

        // Setup layers structure.

        ILayer inputNextLayer = hiddenLayers.Length > 0 ? hiddenLayers[0] as ILayer : outputLayer as ILayer;
        inputLayer.parentNeuralNetwork = this;
        layers[0] = inputLayer;

        for (int i = 1; i < layers.Length - 1; i++)
        {
            layers[i] = hiddenLayers[i - 1];
            hiddenLayers[i - 1].parentNeuralNetwork = this;
        }

        ILayer outputPreviousLayer = hiddenLayers.Length > 0 ? hiddenLayers[hiddenLayers.Length - 1] as ILayer : inputLayer as ILayer;
        outputLayer.parentNeuralNetwork = this;
        layers[layers.Length - 1] = outputLayer;

        // Initing layers.
        inputLayer.Init("Input", null, inputNextLayer);

        for (int i = 1; i < layers.Length - 1; i++)
        {
            hiddenLayers[i - 1].Init("Hidden-" + i, layers[i - 1], layers[i + 1]);
        }

        outputLayer.Init("Output", outputPreviousLayer, inputLayer);
    }

    private void DrawLayers()
    {
        // Getting and Positioning neurons.
        int xAdd = 1;

        YRelative_PositionateObjects(inputLayer.GetNeuronObjects(), xAdd++ * neuronsHorizontalPadding);

        foreach (ILayer hl in hiddenLayers)
        {
            YRelative_PositionateObjects(hl.GetNeuronObjects(), xAdd++ * neuronsHorizontalPadding);
        }

        YRelative_PositionateObjects(outputLayer.GetNeuronObjects(), xAdd++ * neuronsHorizontalPadding);
    }

    void Start()
    {
        foreach (ILayer layer in layers)
        {
            Debug.Log(layer);
            //layer.FeedForward();
        }
        //PredictInput();
    }

    public void PredictInput()
    {
        inputLayer.Predict(InputData);
    }

    public void Train(/*Training Data, */) {
    }

    public void Backpropagate(decimal Cost)
    {

    }

    #region Static

    /* Helpers */

    public static void YRelative_PositionateObjects(GameObject[] gameObjects, float xPos = 0, float pad = 2f)
    {
        //gameObjects[i].transform.position = gameObjects[i].transform.position + new Vector3(0f, i * 1.5f, 0f);
        int length = gameObjects.Length;
        int isOdd = length % 2 == 0 ? 0 : 1;

        float firstYPos = pad * ((isOdd / pad) + (length / 2));
        // gameObjects[0].transform.position.y - gameObjects[0].transform.position.y * 
        for (int i = 0; i < length; i++)
        {
            Vector3 curPos = gameObjects[i].transform.position;
            gameObjects[i].transform.position = new Vector3(curPos.x + xPos, firstYPos - (i * pad), curPos.z);
        }
    }

    /* Instantiatiors */

    public static GameObject CreateNew_Neuron()
    {
        return Instantiate(Resources.Load<GameObject>("Prefabs/Neuron")) as GameObject;
    }

    public static GameObject CreateNew_ConnectionLiner()
    {
        return Instantiate(Resources.Load<GameObject>("Prefabs/ConnectionLiner")) as GameObject;
    }
    #endregion
}
