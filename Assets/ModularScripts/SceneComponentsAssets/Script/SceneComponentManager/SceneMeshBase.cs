using AnchorSharing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class SceneMeshBase : MonoBehaviour
{   
    public SceneMeshData Data { get; private set; }

    protected Mesh mesh;
    
    public void SetData(SceneMeshData data)
    {
        Data = data;
        mesh = generatedMesh(Data);
        initialize();
    }

    private static Mesh generatedMesh(SceneMeshData meshData)
    {
        Mesh mesh = new Mesh();

        if (meshData.vertices.Length >= 65535)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.indices;
        mesh.uv = meshData.uvs;
        mesh.tangents = meshData.tangents;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void OnDestroy()
    {
        clearMesh();
        onDestroyed();
    }

    private void clearMesh()
    {
        if(mesh != null)
        {
            Destroy(mesh);
        }
    }

    protected virtual void onDestroyed() { }
    protected abstract void initialize();
}
