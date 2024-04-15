using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Native;

namespace AnchorSharing
{
    [Serializable]
    public struct SceneMeshData
    {
        public Vector3[] vertices;
        public int[] indices;
        public Vector2[] uvs;
        public Vector4[] tangents; 

        public override string ToString()
        {
            return $"vetex count: {vertices.Length}, index count:{indices.Length}";
        }
    }

    /*
         2----3
         |    |
    back |  --|--> front
         |    |
         0----1
    */
    [Serializable]
    public struct ScenePlaneData
    {
        public byte[] uuID;
        public Vector3[] vertices;
        public Vector3 position;
        public Quaternion rotation;
        public WVR_ScenePlaneLabel sceneLabel;

        public override string ToString()
        {
            return $"id: {uuID}, type:{sceneLabel}, pos: {position}, rot: {rotation}, vertex: {vertices[0]}, {vertices[1]}, {vertices[2]}, {vertices[3]}";
        }

        public static bool operator ==(ScenePlaneData c1, ScenePlaneData c2)
        {
            return c1.Equals(c2);
        }
        public static bool operator !=(ScenePlaneData c1, ScenePlaneData c2)
        {
            return !c1.Equals(c2);
        }

        public override bool Equals(object obj)
        {
            if ((obj is ScenePlaneData) == false) return false;
            ScenePlaneData p2 = (ScenePlaneData)obj;
            return isUUIDEqual(uuID, p2.uuID);
        }

        public override int GetHashCode()
        {
            return -1723399806 + uuID.GetHashCode();
        }

        private static bool isUUIDEqual(byte[] uuid1, byte[] uuid2)
        {
            if (uuid1 == null || uuid2 == null || uuid1.Length != uuid2.Length) return false;
            return uuid1.SequenceEqual(uuid2);
        }
    }
}