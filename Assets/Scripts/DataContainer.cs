using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DataContainer
{
    [Header("Data")]
    public float[] Data;
    public Label[] Labels;

    public void Preprocess()
    {
    }

    public int GetCorrectLabelIndex()
    {
        Label CorrectLabel = Labels[0];
        int RetIdx = 0;

        for (int i = 1; i < Labels.Length; i++)
        {
            if (CorrectLabel.Strength < Labels[i].Strength)
            {
                RetIdx = i;
                CorrectLabel = Labels[i];
            }
        }

        return RetIdx;
    }

    public decimal[] GetData()
    {
        // Convert Data to decimals.
        decimal[] RetData = new decimal[Data.Length];
        for (int i = 0; i < Data.Length; i++)
        {
            RetData[i] = System.Convert.ToDecimal(Data[i]);
        }
        return RetData;
    }

    public void SetData(decimal[] InData)
    {
        float[] NewData = new float[InData.Length];
        for (int i = 0; i < InData.Length; i++)
        {
            NewData[i] = System.Convert.ToSingle(InData[i]);
        }

        Data = NewData;
    }
}

[System.Serializable]
public struct Label
{
    public string FriendlyName;
    public float Strength;
}