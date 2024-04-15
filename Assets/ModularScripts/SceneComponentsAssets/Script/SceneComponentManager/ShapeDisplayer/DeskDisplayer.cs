using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnchorSharing
{
    public class DeskDisplayer : SceneComponentBase
    {
        [SerializeField] private Transform pivotLF;
        [SerializeField] private Transform pivotLN;
        [SerializeField] private Transform pivotRF;
        [SerializeField] private Transform pivotRN;
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private float thickness = 0.05f;

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

            pivotLF.localPosition = new Vector3(v[1].x, v[1].z, v[1].y);
            pivotLN.localPosition = new Vector3(v[3].x, v[3].z, v[3].y);
            pivotRF.localPosition = new Vector3(v[0].x, v[0].z, v[0].y);
            pivotRN.localPosition = new Vector3(v[2].x, v[2].z, v[2].y);

            boxCollider.size = new Vector3(
                Vector3.Distance(v[0], v[1]),                
                Vector3.Distance(v[3], v[1]),
                thickness
            );

            boxCollider.center = new Vector3(
                0,
                0,
                -thickness / 2f
            );
        }
    }
}