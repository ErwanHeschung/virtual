using UnityEngine;

//the root of the creature's DNA
[CreateAssetMenu(menuName = "Evo/CreatureDNA")]
public class CreatureDNA : ScriptableObject
{
    public BodyPartGene root;
    public NeuralNetwork brain;
}
