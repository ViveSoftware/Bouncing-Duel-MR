using AnchorSharing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnchorSharing
{
    public class FloorDisplayer : ScenePlaneBase
    {
        public Vector3[] GetCorners()
        {
            Vector3[] poses = new Vector3[4];
            for(int i=0; i<Data.vertices.Length; ++i)
            {
                poses[i] = transform.TransformPoint(Data.vertices[i]);
            }

            return poses;
        }
    }
}