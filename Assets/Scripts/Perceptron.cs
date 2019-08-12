using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct Perceptron
{
    public SafeDecimal[] Weights;
    public SafeDecimal Bias;
    public string PerceptronName { get; private set; }

    public decimal CurrentActivation;
    public decimal CurrentLinearZ;

    // Extra
    public GameObject NeuronObj;
    private readonly GameObject[] ConnectionLiners;

    public LineRenderer GetConnectionLiner(int ConnectionIdx)
    {
        if (NeuronObj == null)
        {
            Debug.Log("No NeuronObj for perceptron " + PerceptronName + ".");
            return null;
        }

        GameObject linerObj;

        if (ConnectionLiners[ConnectionIdx] != null)
        {
            linerObj = ConnectionLiners[ConnectionIdx];
        } else
        {
            linerObj = NeuralNetwork.CreateNew_ConnectionLiner();
            linerObj.transform.parent = NeuronObj.transform;
            linerObj.name = string.Format("Liner-{0}", ConnectionIdx);
        }

        LineRenderer lr = linerObj.GetComponent<LineRenderer>();

        Material activationMat = lr.material;
        activationMat.color = new Color(1f, 1f, 1f, System.Convert.ToSingle(CurrentActivation) + 0.1f);

        ConnectionLiners[ConnectionIdx] = linerObj;

        return lr;
    }

    public void UpdateActivationText()
    {
        if (NeuronObj == null)
        {
            Debug.Log("No NeuronObj for perceptron " + PerceptronName + ".");
            return;
        }

        GameObject ActivationObj = NeuronObj.transform.Find("UI/Activation").gameObject;
        ActivationObj.GetComponent<Text>().text = CurrentActivation.ToString();
    }

    public Perceptron(string perceptronName, decimal[] weights, decimal bias)
    {
        PerceptronName = perceptronName.Equals(string.Empty)? "_Perceptron" : perceptronName;
        CurrentActivation = 0m;
        CurrentLinearZ = 0m;

        // Weights init.
        int newWeightsLength = weights.Length;
        Weights = new SafeDecimal[newWeightsLength];

        for (int i = 0; i < newWeightsLength; i++)
        {
            Weights[i].Value = System.Convert.ToDecimal(Random.Range(-1f, 1f));//weights[i];
        }

        // Bias init.
        Bias = new SafeDecimal(System.Convert.ToDecimal(Random.Range(-1f, 1f)));//bias

        // Extra
        NeuronObj = null;
        ConnectionLiners = new GameObject[newWeightsLength];
    }

    public decimal Compute(decimal[] Input, System.Func<decimal[], decimal[]> ActivationFunction)
    {
        decimal[] Z = new decimal[1];

        int weightsLength = Weights.Length;
        int inputLength = Input.Length;

        if (Weights.Length != Input.Length)
        {
            Debug.LogError(string.Format("Trying to compute perceptron {0} where Weights' Length = {1} and Input's Length = {2}.", PerceptronName, weightsLength, inputLength));
            return Z[0];
        }

        // Mult of inputs with their corresponding weights.
        for (int i = 0; i < inputLength; i++)
        {
            Z[0] += Input[i] * Weights[i].Value;
        }

        // Adding the bias.
        Z[0] += Bias.Value;
        CurrentLinearZ = Z[0];

        // Applying the activation function.
        if (ActivationFunction != null)
        {
            Z = ActivationFunction(Z);
            // Save the perceptron's activation.
            CurrentActivation = Z[0];
            return CurrentActivation;
        }

        return Z[0];
    }

    public decimal GetZ()
    {
        return CurrentLinearZ;
    }
}

public struct SafeDecimal
{
    private decimal _value;
    public decimal Value {
        get { return _value; }
        set { _value = (value < -1) ? -1 : (value > 1) ? 1 : value; }
    }

    public SafeDecimal(decimal value)
    {
        _value = value;
        Value = value;
    }
}