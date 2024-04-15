
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Wave.Essence.ScenePerception.ScenePerceptionManager;

namespace AnchorSharing
{
    public abstract class ScenePlaneBase : SceneComponentBase
    {
        [SerializeField] protected MeshFilter meshFilter;

        [SerializeField] protected LineRenderer borderRenderer;
        [SerializeField] protected float borderWidth = 0.05f;

        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private float thickness = 0.01f;

        protected override void initialize()
        {
            generateMesh(); 
            generateBorder();            
        }

        protected void generateMesh()
        {
            meshFilter.mesh = MeshGenerationHelper.GenerateQuadMesh(Data.vertices);

            boxCollider.size = new Vector3(
               Vector3.Distance(Data.vertices[0], Data.vertices[1]),
               Vector3.Distance(Data.vertices[3], Data.vertices[1]),
               thickness
            );

            boxCollider.center = new Vector3(
                0,
                0,
                -thickness / 2f
            );
        }

        //[Button("Apply line properties")]
        protected void generateBorder()
        {
            if (borderRenderer == null) return;

            borderRenderer.widthMultiplier = borderWidth;
            borderRenderer.alignment = LineAlignment.View;
            borderRenderer.useWorldSpace = false;
            borderRenderer.positionCount = 4;
            if (Data != null && Data.vertices.Length == 4)
            {
                borderRenderer.SetPositions(new Vector3[]
                {
                    Data.vertices[0],
                    Data.vertices[1],
                    Data.vertices[3],
                    Data.vertices[2],
                });
            }
            else
            {
                borderRenderer.SetPositions(new Vector3[]
                {
                    new Vector3(0.5f, -0.5f, 0),
                    new Vector3(0.5f, 0.5f, 0),
                    new Vector3(-0.5f, 0.5f, 0),
                    new Vector3(-0.5f, -0.5f, 0),
                });
            }
            borderRenderer.loop = true;
        }

        protected override void updateData()
        {
            generateMesh();
            generateBorder();
        }

        private void OnDestroy()
        {
            if (meshFilter != null && meshFilter.sharedMesh)
            {
                Destroy(meshFilter.sharedMesh);
            }
        }
    }
}
