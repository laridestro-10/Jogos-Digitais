using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TorusGenerator : MonoBehaviour
{
    [Header("Configurações do Anel")]
    public float radius = 0.8f;
    public float tubeRadius = 0.2f;
    public int radialSegments = 32;
    public int tubularSegments = 16;

    void Start()
    {
        GenerateTorus();
    }

    void OnValidate()
    {
        
        if (gameObject.activeInHierarchy)
        {
            GenerateTorus();
        }
    }

    public void GenerateTorus()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) return;

        Mesh mesh = new Mesh();
        mesh.name = "TorusProcedural";

        int numVertices = (radialSegments + 1) * (tubularSegments + 1);
        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uv = new Vector2[numVertices];
        int[] triangles = new int[radialSegments * tubularSegments * 6];

        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i <= radialSegments; i++)
        {
            float u = (float)i / radialSegments * Mathf.PI * 2.0f;
            float cosU = Mathf.Cos(u);
            float sinU = Mathf.Sin(u);

            for (int j = 0; j <= tubularSegments; j++)
            {
                float v = (float)j / tubularSegments * Mathf.PI * 2.0f;
                float cosV = Mathf.Cos(v);
                float sinV = Mathf.Sin(v);

                float x = (radius + tubeRadius * cosV) * cosU;
                float y = tubeRadius * sinV;
                float z = (radius + tubeRadius * cosV) * sinU;

                vertices[vertIndex] = new Vector3(x, y, z);
                uv[vertIndex] = new Vector2((float)i / radialSegments, (float)j / tubularSegments);
                vertIndex++;
            }
        }

        for (int i = 0; i < radialSegments; i++)
        {
            for (int j = 0; j < tubularSegments; j++)
            {
                int nextI = (i + 1);
                int nextJ = (j + 1);

                int current = i * (tubularSegments + 1) + j;
                int nextRow = nextI * (tubularSegments + 1) + j;

                triangles[triIndex++] = current;
                triangles[triIndex++] = nextRow;
                triangles[triIndex++] = nextI * (tubularSegments + 1) + nextJ;

                triangles[triIndex++] = current;
                triangles[triIndex++] = nextI * (tubularSegments + 1) + nextJ;
                triangles[triIndex++] = i * (tubularSegments + 1) + nextJ;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        mf.sharedMesh = mesh;

        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider != null)
        {
            collider.sharedMesh = mesh;
        }

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null && mr.sharedMaterial == null)
        {
            mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        }
    }
}