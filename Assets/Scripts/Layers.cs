using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface ILayer
{
    void Init(string layerName, ILayer previousLayer, ILayer nextLayer);
    decimal[] GetInput();
    DataContainer? ComputeLayer(decimal[] Input);
    int GetPerceptronsCount();
    string GetLayerName();
    GameObject[] GetNeuronObjects();
    void Backpropagate(decimal[] Cost, decimal BPrefix = 0); // 0 means that this should calculate the prefix itself.
}

[System.Serializable]
public class InputLayer : ILayer
{
    #region Settings
    [Range(2, 999)]
    public int InputSize = 2;
    #endregion

    #region Private
    [HideInInspector]
    public DataContainer InputData;
    float[] f_Input;
    decimal[] m_Input;
    bool IsExecuting = false;
    private string LayerName;
    private ILayer NextLayer;
    private GameObject[] NeuronObjects;

    [HideInInspector]
    public NeuralNetwork parentNeuralNetwork;
    #endregion

    public void Init(string Name, ILayer PrevLayer, ILayer NextLayer)
    {
        this.LayerName = Name;
        this.NextLayer = NextLayer;

        // Convert Input to decimals.
        m_Input = new decimal[InputSize];
    }

    public decimal[] GetInput()
    {
        return m_Input;
    }

    public DataContainer? ComputeLayer(decimal[] Input)
    {
        if (NeuronObjects != null)
        {
            for (int i = 0; i < NeuronObjects.Length; i++)
            {
                GameObject ActivationObj = NeuronObjects[i].transform.Find("UI/Activation").gameObject;
                ActivationObj.GetComponent<Text>().text = Input[i].ToString();
            }
        }

        if (IsExecuting)
        {
            Debug.Log("Trying to Feed Forward but the neural network is executing.");
            return null;
        }

        m_Input = Input;

        IsExecuting = true;

        var Result = NextLayer.ComputeLayer(Input);

        IsExecuting = false;

        return Result;
    }

    public void Predict(DataContainer InInputData)
    {
        InputData = InInputData;

        // Convert Input to decimals.
        m_Input = new decimal[InputData.Data.Length];
        for (int i = 0; i < InputData.Data.Length; i++)
        {
            m_Input[i] = System.Convert.ToDecimal(InputData.Data[i]);
        }

        ComputeLayer(GetInput());
    }

    public int GetPerceptronsCount()
    {
        return m_Input.Length;
    }

    public GameObject[] GetNeuronObjects()
    {
        if (NeuronObjects != null)
            return NeuronObjects;

        NeuronObjects = new GameObject[m_Input.Length];

        for (int i = 0; i < m_Input.Length; i++)
        {
            NeuronObjects[i] = NeuralNetwork.CreateNew_Neuron();
        }

        return NeuronObjects;
    }

    public string GetLayerName()
    {
        return LayerName;
    }

    public override string ToString()
    {
        return LayerName;
    }

    public void Backpropagate(decimal[] Cost, decimal BPrefix = 0)
    {
        //throw new NotImplementedException();
    }
}

[System.Serializable]
public class HiddenLayer : ILayer
{
    #region Settings
    [Range(1, 999)]
    public int PerceptronsCount = 1;
    Perceptron[] Perceptrons;

    public ActivationFunctions.EActivationFunction LayerActivationFunction;
    System.Func<decimal[], decimal[]> ActivationFunction;
    #endregion

    #region Private
    private string LayerName;
    private ILayer PreviousLayer;
    private ILayer NextLayer;
    private GameObject[] NeuronObjects;

    [HideInInspector]
    public NeuralNetwork parentNeuralNetwork;
    #endregion

    public void Init(string layerName, ILayer previousLayer, ILayer nextLayer)
    {
        this.LayerName = layerName;
        this.PreviousLayer = previousLayer;
        this.NextLayer = nextLayer;

        this.Perceptrons = new Perceptron[PerceptronsCount];

        //System.Random random = new System.Random();

        for (int i = 0; i < PerceptronsCount; i++)
        {
            // Generate random weights and bias.
            //...

            Perceptrons[i] = new Perceptron(string.Format("P_{0}_{1}", LayerName, i),
                new decimal[PreviousLayer.GetInput().Length],
                0m);
        }

        ActivationFunction = ActivationFunctions.GetAppropriateActivationFunction(LayerActivationFunction);
    }

    public decimal[] GetInput()
    {
        if (Perceptrons == null || Perceptrons.Length == 0)
        {
            Debug.LogError(string.Format("Hidden Layer {0} has no valid perceptrons, can't get perceptrons values...", LayerName));
            return null;
        }

        // Check for:
        // Activation Function.
        // Previous Layers' Input?

        int perceptronsLength = Perceptrons.Length;

        decimal[] PerceptronActivations = new decimal[perceptronsLength];

        // For every perceptron, we compute the weighted-sum + bias and with the applied activation function.
        for (int i = 0; i < perceptronsLength; i++)
        {
            PerceptronActivations[i] = Perceptrons[i].CurrentActivation;
        }

        return PerceptronActivations;
    }

    public DataContainer? ComputeLayer(decimal[] Input)
    {
        if (NextLayer == null)
        {
            Debug.LogError(string.Format("Unvalid next layer for Hidden Layer {0}.", LayerName));
            return null;
        }

        if (Perceptrons == null || Perceptrons.Length == 0)
        {
            Debug.LogError(string.Format("Hidden Layer {0} has no valid perceptrons, can't get perceptrons values...", LayerName));
            return null;
        }
        
        // Check for:
        // Activation Function.
        // Previous Layers' Input?

        int perceptronsLength = Perceptrons.Length;

        decimal[] PerceptronValues = new decimal[perceptronsLength];

        // For every perceptron, we compute the weighted-sum + bias and with the applied activation function.
        for (int i = 0; i < perceptronsLength; i++)
        {
            PerceptronValues[i] = Perceptrons[i].Compute(Input, ActivationFunction);
        }

        // Cosmetics at the end.

        if (NeuronObjects != null)
        {
            for (int i = 0; i < Perceptrons.Length; i++)
            {
                GameObject NeuronObj = Perceptrons[i].NeuronObj;

                for (int j = 0; j < PreviousLayer.GetNeuronObjects().Length; j++)
                {
                    GameObject OtherNeuronObj = PreviousLayer.GetNeuronObjects()[j];

                    LineRenderer lr = Perceptrons[i].GetConnectionLiner(j);
                    Perceptrons[i].UpdateActivationText();
                    lr.SetPositions(new Vector3[] { NeuronObj.transform.position, OtherNeuronObj.transform.position });
                }
            }
        }

        return NextLayer.ComputeLayer(GetInput());
    }

    public int GetPerceptronsCount()
    {
        return Perceptrons.Length;
    }

    public GameObject[] GetNeuronObjects()
    {
        if (NeuronObjects != null)
            return NeuronObjects;

        NeuronObjects = new GameObject[Perceptrons.Length];

        for (int i = 0; i < Perceptrons.Length; i++)
        {
            Perceptrons[i].NeuronObj = NeuronObjects[i] = NeuralNetwork.CreateNew_Neuron();
        }

        return NeuronObjects;
    }

    public string GetLayerName()
    {
        return LayerName;
    }

    public override string ToString()
    {
        return GetLayerName();
    }

    public void Backpropagate(decimal[] Cost, decimal BPrefix = 0)
    {
        decimal[] NonLinearZs = new decimal[Perceptrons.Length];

        for (int i = 0; i < Perceptrons.Length; i++)
        {
            NonLinearZs[i] = Perceptrons[i].GetZ();
        }

        for (int i = 0; i < Perceptrons.Length; i++)
        {
            Perceptron P = Perceptrons[i];

            // Prepare the prefix being: (dC0/da0) * (da0/dZ0) 
            BPrefix *= ActivationFunctions.GetAppropriateDerivativeActivationFunction(LayerActivationFunction)
                (new decimal[] { P.GetZ() }, i)[0];
            BPrefix *= P.CurrentActivation;

            Debug.Log(BPrefix);

            // Update current weights.
            for (int j = 0; j < P.Weights.Length; j++)
            {
                decimal LR = Convert.ToDecimal(parentNeuralNetwork.LearningRate);
                P.Weights[j].Value -= LR * BPrefix * PreviousLayer.GetInput()[j];
            }

            // Tell last layer to propagate using this perceptron's relative prefix.
            PreviousLayer.Backpropagate(Cost, BPrefix);
        }
    }
}

[System.Serializable]
public class OutputLayer : ILayer
{
    #region Settings
    public int ClassesNumber = 1;
    Perceptron[] Perceptrons;

    public ActivationFunctions.EActivationFunction LayerActivationFunction;
    System.Func<decimal[], decimal[]> ActivationFunction;
    #endregion

    #region Private
    private string LayerName;
    private GameObject[] NeuronObjects;
    private ILayer PreviousLayer;
    private ILayer NextLayer;

    [HideInInspector]
    public NeuralNetwork parentNeuralNetwork;
    private Label[] LastLabels;
    #endregion

    public void Init(string layerName, ILayer previousLayer, ILayer nextLayer)
    {
        this.LayerName = layerName;
        this.PreviousLayer = previousLayer;
        this.NextLayer = nextLayer;

        this.Perceptrons = new Perceptron[ClassesNumber];

        //System.Random random = new System.Random();

        for (int i = 0; i < ClassesNumber; i++)
        {
            // Generate random weights and bias.
            //...

            Perceptrons[i] = new Perceptron(string.Format("P_{0}_{1}", LayerName, i),
                new decimal[PreviousLayer.GetInput().Length],
                0m);
        }

        ActivationFunction = ActivationFunctions.GetAppropriateActivationFunction(LayerActivationFunction);
    }

    public decimal[] GetInput()
    {
        return null;
    }

    public DataContainer? ComputeLayer(decimal[] Input)
    {
        int perceptronsLength = Perceptrons.Length;

        decimal[] PerceptronValues = new decimal[perceptronsLength];

        // For every perceptron, we compute the weighted-sum + bias and with the applied activation function.
        for (int i = 0; i < perceptronsLength; i++)
        {
            PerceptronValues[i] = Perceptrons[i].Compute(Input, null);
        }

        decimal[] Output = ActivationFunction(PerceptronValues);

        DataContainer OutputData = new DataContainer();
        OutputData.SetData(Output);

        decimal max = Output[0];
        int indexMax = 0; 

        for(int i = 0; i < Output.Length; i++)
        {
            var O = Output[i];

            if (O > max)
            {
                max = O;
                indexMax = i;
            }
        }

        InputLayer inputLayer = this.NextLayer as InputLayer;

        LastLabels = inputLayer.InputData.Labels;

        string LabelName = LastLabels[indexMax].FriendlyName;
        string CorrectLabelName = LastLabels[inputLayer.InputData.GetCorrectLabelIndex()].FriendlyName;

        Debug.Log("***BEGIN OUTPUT LOG***");
        Debug.Log(string.Format("Output Layer Prediction: {0}.", LabelName));
        Debug.Log(string.Format("We made a mistake, we should've said: {0}.", CorrectLabelName));

        decimal Cost = 0m;
        
        for (int i = 0; i < ClassesNumber; i++)
        {
            decimal C = (Output[i] - Convert.ToDecimal(inputLayer.InputData.Labels[i].Strength));
            Cost += C *= C;
        }

        Debug.Log(string.Format("We have a cost of: {0}.", Cost));
        Debug.Log("Backpropagating...");
        parentNeuralNetwork.Backpropagate(Cost);
        Backpropagate(null);

        Debug.Log("***END OUTPUT LOG***");

        // Cosmetics at the end.

        if (NeuronObjects != null)
        {
            for (int i = 0; i < Perceptrons.Length; i++)
            {
                GameObject NeuronObj = Perceptrons[i].NeuronObj;

                // Explicitly updating current activation, as we're using logistic activation function...
                // This is for big optimization over readibility...
                Perceptrons[i].CurrentActivation = Output[i];

                GameObject ActivationObj = Perceptrons[i].NeuronObj.transform.Find("UI/Activation").gameObject;
                ActivationObj.GetComponent<Text>().text = Output[i].ToString();

                for (int j = 0; j < PreviousLayer.GetNeuronObjects().Length; j++)
                {
                    GameObject OtherNeuronObj = PreviousLayer.GetNeuronObjects()[j];

                    LineRenderer lr = Perceptrons[i].GetConnectionLiner(j);
                    Perceptrons[i].UpdateActivationText();
                    lr.SetPositions(new Vector3[] { NeuronObj.transform.position, OtherNeuronObj.transform.position });
                }
            }
        }

        return OutputData;
    }

    public int GetPerceptronsCount()
    {
        return 0;
    }

    public GameObject[] GetNeuronObjects()
    {
        if (NeuronObjects != null)
            return NeuronObjects;

        NeuronObjects = new GameObject[Perceptrons.Length];

        for (int i = 0; i < Perceptrons.Length; i++)
        {
            Perceptrons[i].NeuronObj = NeuronObjects[i] = NeuralNetwork.CreateNew_Neuron();
        }

        return NeuronObjects;
    }

    public string GetLayerName()
    {
        return LayerName;
    }

    public override string ToString()
    {
        return GetLayerName();
    }

    public void Backpropagate(decimal[] Cost, decimal BPrefix = 0)
    {
        decimal[] NonLinearZs = new decimal[Perceptrons.Length];

        for (int i = 0; i < Perceptrons.Length; i++)
        {
            NonLinearZs[i] = Perceptrons[i].GetZ();
        }
        
        for (int i = 0; i < Perceptrons.Length; i++)
        {
            Perceptron P = Perceptrons[i];

            BPrefix = 1m;

            // Prepare the prefix being: (dC0/da0) * (da0/dZ0) 
            BPrefix *= 2 * (P.CurrentActivation - Convert.ToDecimal(LastLabels[i].Strength));
            BPrefix *= ActivationFunctions.GetAppropriateDerivativeActivationFunction(LayerActivationFunction)
                (NonLinearZs, i)[i];

            // Update current weights.
            for (int j = 0; j < P.Weights.Length; j++)
            {
                decimal LR = Convert.ToDecimal(parentNeuralNetwork.LearningRate);
                P.Weights[j].Value -= LR * BPrefix * PreviousLayer.GetInput()[j];
            }

            // Tell last layer to propagate using this perceptron's relative prefix.
            PreviousLayer.Backpropagate(Cost, BPrefix);
        }
    }
}
