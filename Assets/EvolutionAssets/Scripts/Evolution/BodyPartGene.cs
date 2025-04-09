using System.Collections.Generic;
using UnityEngine;

public class BodyPartGene
{
    public Vector3 offset;
    public Vector3 rotation;
    public Vector3 scale;
    public JointType jointType;
    public float jointStrength;
    public float frequency;
    public float amplitude;
    public float phase;

    public List<BodyPartGene> children = new List<BodyPartGene>();
}

