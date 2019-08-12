using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActivationFunctions
{
    private const decimal One = 1.0m;

    public enum EActivationFunction
    {
        Sigmoid,
        Softmax
    }

    // Sigmoid
    public static decimal[] Sigmoid(decimal[] Z) => new decimal[] { One / (One + System.Convert.ToDecimal(Mathf.Exp(-System.Convert.ToSingle(Z[0])))) };
    public static decimal[] Sigmoid_Derivative(decimal[] Z, int RespectIdx = 0) {
        decimal[] RetVal = Sigmoid(Z);

        RetVal[0] = RetVal[0] - RetVal[0] * RetVal[0];

        return RetVal;
    }

    // Softmax
    public static decimal[] Softmax(decimal[] Z) {
        decimal[] RetVal = new decimal[Z.Length];
        decimal Div = 0m;

        for (int i = 0; i < Z.Length; i++)
        {
            RetVal[i] = System.Convert.ToDecimal(Mathf.Exp(System.Convert.ToSingle(Z[i])));
            Div += RetVal[i];
        }

        for (int i = 0; i < Z.Length; i++)
        {
            RetVal[i] /= Div;
        }

        return RetVal;
    }
    public static decimal[] Softmax_Derivative(decimal[] Z, int RespectIdx = 0) {
        decimal[] S = Softmax(Z);
        decimal[] RetVal = new decimal[S.Length];

        for (int i = 0; i < S.Length; i++)
        {
            if (RespectIdx == i)
            {
                RetVal[i] = S[RespectIdx] * (1 - S[i]);
            }
            else
            {
                RetVal[i] = - S[RespectIdx] * S[i];
            }
        }

        return RetVal;
    }

    #region Helpers
    public static System.Func<decimal[], decimal[]> GetAppropriateActivationFunction(EActivationFunction ActivationFunctionName)
    {
        System.Func<decimal[], decimal[]> RetVal;

        switch (ActivationFunctionName)
        {
            case EActivationFunction.Sigmoid:
                RetVal = Sigmoid;
                break;
            case EActivationFunction.Softmax:
                RetVal = Softmax;
                break;
            default:
                RetVal = Sigmoid;
                break;
        }

        return RetVal;
    }
    public static System.Func<decimal[], int, decimal[]> GetAppropriateDerivativeActivationFunction(EActivationFunction DerivativeActivationFunctionName)
    {
        System.Func<decimal[], int, decimal[]> RetVal;

        switch (DerivativeActivationFunctionName)
        {
            case EActivationFunction.Sigmoid:
                RetVal = Sigmoid_Derivative;
                break;
            case EActivationFunction.Softmax:
                RetVal = Softmax_Derivative;
                break;
            default:
                RetVal = Sigmoid_Derivative;
                break;
        }

        return RetVal;
    }

    #endregion
}
