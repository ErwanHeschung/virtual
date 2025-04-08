using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class CreatureDNA
{
    public int limbCount = 2;
    public List<LimbGene> limbs = new List<LimbGene>();
}

public enum JointType { Fixed, Hinge, Spring }

[System.Serializable]
public class LimbGene
{
    public Vector3 positionOffset;
    public Vector3 scale = Vector3.one;
    public float amplitude = 30f;
    public float frequency = 1f;
    public float phase = 0f;

    public JointType jointType;
    public float jointStrength;
}

