using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Ocean : MonoBehaviour
{
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    private Vector2 uvScale;

    public int xSize = 20;
    public int zSize = 20;

    public bool originateMeshAtOrigin;
    public bool showGizmos = false;
    private float adjustedXPos;
    private float adjustedZPos;

    private Mesh mesh;


    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        adjustedXPos = 0;
        adjustedZPos = 0;
        if (!originateMeshAtOrigin)
        {
            adjustedXPos = xSize / 2;
            adjustedZPos = zSize / 2;
        }
 
        CreateShape();
        UpdateMesh();
     }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        uvs = new Vector2[vertices.Length];
         
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                vertices[i] = new Vector3(x - adjustedXPos, 0, z - adjustedZPos);
                uvs[i] = new Vector2((float)x/xSize - adjustedXPos, (float)z/zSize - adjustedZPos);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }

            vert++;
        }
        
 
   }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        
        mesh.RecalculateNormals();
    }


    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            if (vertices == null)
                return;

            Vector3 recalculatedXYZVertex = new Vector3();
            for (int i = 0; i < vertices.Length; i++)
            {
                recalculatedXYZVertex.x = vertices[i].x * transform.localScale.x;
                recalculatedXYZVertex.y = vertices[i].y * transform.localScale.y;
                recalculatedXYZVertex.z = vertices[i].z * transform.localScale.z;

                Gizmos.DrawSphere(recalculatedXYZVertex, .1f);
            }
        }
    }
}