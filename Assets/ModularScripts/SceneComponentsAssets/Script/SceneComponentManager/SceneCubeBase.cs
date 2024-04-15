using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnchorSharing
{
    public class SceneCubeBase : SceneComponentBase
    {
        [SerializeField] private Transform pivotBLF;
        [SerializeField] private Transform pivotBLN;
        [SerializeField] private Transform pivotBRF;
        [SerializeField] private Transform pivotBRN;
        [SerializeField] private Transform pivotTLF;
        [SerializeField] private Transform pivotTLN;
        [SerializeField] private Transform pivotTRF;
        [SerializeField] private Transform pivotTRN;
        [SerializeField] private BoxCollider boxCollider;

        protected override void initialize()
        {
            refreshBone();
        }

        protected override void updateData()
        {
            refreshBone();
        }

        private void refreshBone()
        {
            Vector3[] v = Data.vertices;

            pivotTLF.localPosition = new Vector3(v[1].x, v[1].z, v[1].y);
            pivotTLN.localPosition = new Vector3(v[3].x, v[3].z, v[3].y);
            pivotTRF.localPosition = new Vector3(v[0].x, v[0].z, v[0].y);
            pivotTRN.localPosition = new Vector3(v[2].x, v[2].z, v[2].y);

            pivotBLF.position = Vector3.Scale(pivotTLF.position, new Vector3(1, 0, 1));
            pivotBLN.position = Vector3.Scale(pivotTLN.position, new Vector3(1, 0, 1));
            pivotBRF.position = Vector3.Scale(pivotTRF.position, new Vector3(1, 0, 1));
            pivotBRN.position = Vector3.Scale(pivotTRN.position, new Vector3(1, 0, 1));

            boxCollider.size = new Vector3(
                Vector3.Distance(v[0], v[1]),
                Vector3.Distance(v[1], v[3]),
                transform.position.y
            );

            boxCollider.center = new Vector3(
                0,
                0,
                Vector3.Dot(transform.forward, Vector3.up) > 0 ? -transform.position.y / 2f : transform.position.y / 2f
            );

            NavMeshObstacle obstacle = gameObject.AddComponent<NavMeshObstacle>();
            obstacle.center = boxCollider.center;
            obstacle.size = boxCollider.size;
        }
    }
}