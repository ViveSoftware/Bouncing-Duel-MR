using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnchorSharing
{
    public class SceneMesh : SceneMeshBase
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshCollider meshCollider;        

        protected override void initialize()
        {
            if(meshCollider != null)
            {
                meshCollider.sharedMesh = mesh;
            }
            
            if(meshFilter != null)
            {
                meshFilter.sharedMesh = mesh;
            }
        }

        protected override void onDestroyed()
        {
            Debug.Log($"meshCollider.sharedMesh == null = {meshCollider != null && meshCollider.sharedMesh == null}");
            Debug.Log($"meshFilter.sharedMesh == null = {meshFilter != null && meshFilter.sharedMesh == null}");
        }
    }
}