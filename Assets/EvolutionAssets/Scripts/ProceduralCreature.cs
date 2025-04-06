using UnityEngine;

public class ProceduralCreature
{
    private GameObject gameObject;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    public ProceduralCreature(GameObject obj)
    {
        this.gameObject = obj;
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

        mesh = new Mesh();
        meshFilter.mesh = mesh;

        // Use Unity's default material (should work without pink issue)
        Material creatureMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        creatureMaterial.color = new Color(Random.value, Random.value, Random.value);
        meshRenderer.material = creatureMaterial;


        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        gameObject.AddComponent<CreatureAgent>();

        GenerateCube();
    }

    void GenerateCube()
    {
        vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f, -0.5f),
            new Vector3(-0.5f,  0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f,  0.5f),
            new Vector3( 0.5f, -0.5f,  0.5f),
            new Vector3( 0.5f,  0.5f,  0.5f),
            new Vector3(-0.5f,  0.5f,  0.5f)
        };

        triangles = new int[]
        {
            0, 2, 1, 0, 3, 2,
            4, 5, 6, 4, 6, 7,
            0, 1, 5, 0, 5, 4,
            3, 7, 6, 3, 6, 2,
            0, 4, 7, 0, 7, 3,
            1, 2, 6, 1, 6, 5
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
