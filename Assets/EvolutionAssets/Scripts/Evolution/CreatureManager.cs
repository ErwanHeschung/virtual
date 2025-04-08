using UnityEngine;

public class CreatureManager : MonoBehaviour
{
    public GameObject creaturePrefab;
    public GameObject limbPrefab;

    void Start()
    {
        GameObject creature = new GameObject("Creature");
        var builder = creature.AddComponent<CreatureBuilder>();
        builder.limbPrefab = limbPrefab;

        var dna = new CreatureDNA();
        dna.limbCount = 4;

        for (int i = 0; i < dna.limbCount; i++)
        {
            dna.limbs.Add(new LimbGene
            {
                positionOffset = new Vector3(1.5f * i, 0, 0),
                scale = Vector3.one,
                amplitude = Random.Range(10f, 40f),
                frequency = Random.Range(0.5f, 2f),
                phase = Random.Range(0f, Mathf.PI * 2f)
            });
        }

        builder.dna = dna;
        builder.Build();
    }
}
