using UnityEngine;

[System.Serializable]
public class NeuralNetwork
{
    public int inputCount = 4;
    public int hiddenCount = 4;
    public int outputCount = 3;

    public float[] inputs;
    public float[] hidden;
    public float[] outputs;

    public float[,] weightsInputHidden;
    public float[,] weightsHiddenOutput;

    public void Initialize()
    {
        inputs = new float[inputCount];
        hidden = new float[hiddenCount];
        outputs = new float[outputCount];

        weightsInputHidden = new float[inputCount, hiddenCount];
        weightsHiddenOutput = new float[hiddenCount, outputCount];

        RandomizeWeights();
    }

    public void RandomizeWeights()
    {
        for (int i = 0; i < inputCount; i++)
        {
            for (int j = 0; j < hiddenCount; j++)
            {
                weightsInputHidden[i, j] = UnityEngine.Random.Range(-1f, 1f);
            }
        }
        for (int i = 0; i < hiddenCount; i++)
        {
            for (int j = 0; j < outputCount; j++)
            {
                weightsHiddenOutput[i, j] = UnityEngine.Random.Range(-1f, 1f);
            }
        }
    }

    public NeuralNetwork Clone()
    {
        NeuralNetwork clone = new NeuralNetwork();
        clone.inputCount = this.inputCount;
        clone.hiddenCount = this.hiddenCount;
        clone.outputCount = this.outputCount;

        clone.Initialize();

        for (int i = 0; i < inputCount; i++)
        {
            for (int j = 0; j < hiddenCount; j++)
            {
                clone.weightsInputHidden[i, j] = this.weightsInputHidden[i, j];
            }
        }
        for (int i = 0; i < hiddenCount; i++)
        {
            for (int j = 0; j < outputCount; j++)
            {
                clone.weightsHiddenOutput[i, j] = this.weightsHiddenOutput[i, j];
            }
        }
        return clone;
    }

    public float[] FeedForward(float[] inputValues)
    {
        if (inputs == null || hidden == null || outputs == null)
        {
            return new float[outputCount];
        }

        for (int h = 0; h < hiddenCount; h++)
        {
            hidden[h] = 0;
            for (int i = 0; i < inputCount; i++)
                hidden[h] += inputValues[i] * weightsInputHidden[i, h];
            hidden[h] = (float)System.Math.Tanh(hidden[h]);
        }

        for (int o = 0; o < outputCount; o++)
        {
            outputs[o] = 0;
            for (int h = 0; h < hiddenCount; h++)
                outputs[o] += hidden[h] * weightsHiddenOutput[h, o];
            outputs[o] = (float)System.Math.Tanh(outputs[o]);
        }

        return outputs;
    }

    public NeuralNetwork CloneAndMutate()
    {
        NeuralNetwork clone = Clone();
        for (int i = 0; i < clone.inputCount; i++)
        {
            for (int j = 0; j < clone.hiddenCount; j++)
            {
                if (Random.value < 0.2f)
                {
                    clone.weightsInputHidden[i, j] += Random.Range(-0.1f, 0.1f);
                }
            }
        }
        for (int i = 0; i < clone.hiddenCount; i++)
        {
            for (int j = 0; j < clone.outputCount; j++)
            {
                if (Random.value < 0.2f)
                {
                    clone.weightsHiddenOutput[i, j] += Random.Range(-0.1f, 0.1f);
                }
            }
        }
        return clone;
    }
}