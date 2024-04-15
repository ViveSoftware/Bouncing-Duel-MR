using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnchorSharing
{
    [Serializable]
    public struct SceneStatus
    {
        public MapStatus mapStatus;
        public SceneMeshData[] colliderMeshes;
        public SceneMeshData[] visualMeshes;
        public ScenePlaneData[] scenePlanes;

        public static readonly SceneStatus Empty;

        public override string ToString()
        {
            string str = "";

            str += $"map info: {mapStatus}\n";

            if (scenePlanes != null)
            {
                foreach (ScenePlaneData plane in scenePlanes)
                {
                    str += $"scene plane: {plane}\n";
                }
            }

            if(colliderMeshes != null)
            {
                foreach (SceneMeshData mesh in colliderMeshes)
                {
                    str += $"collider scene mesh: {mesh}";
                }
            }

            if (visualMeshes != null)
            {
                foreach (SceneMeshData mesh in visualMeshes)
                {
                    str += $"visual scene mesh: {mesh}";
                }
            }

            return str;
        }
    }

    [Serializable]
    public struct MapStatus
    {
        public int keyFrame;
        public int map_fov_percentage;
        public int map_status;
        public int featurepoint;
        public int front_cam_points;
        public int map_id;
        public int map_number;
        public float current_height;
        public float original_height;
        public int lost_tracking_reason;

        public bool IsValid
        {
            get
            {
                return !Equals(INVALID);
            }
        }

        public override string ToString()
        {
            return $"id: {map_id}";
        }

        public static readonly MapStatus INVALID = new MapStatus { map_id = -1 };
        public static bool operator ==(MapStatus c1, MapStatus c2)
        {
            return c1.Equals(c2);
        }
        public static bool operator !=(MapStatus c1, MapStatus c2)
        {
            return !c1.Equals(c2);
        }
        public override bool Equals(object obj)
        {
            return obj is MapStatus status &&
                   keyFrame == status.keyFrame &&
                   map_fov_percentage == status.map_fov_percentage &&
                   map_status == status.map_status &&
                   featurepoint == status.featurepoint &&
                   front_cam_points == status.front_cam_points &&
                   map_id == status.map_id &&
                   map_number == status.map_number &&
                   current_height == status.current_height &&
                   original_height == status.original_height &&
                   lost_tracking_reason == status.lost_tracking_reason;
        }
        public override int GetHashCode()
        {
            int hashCode = -1316069930;
            hashCode = hashCode * -1521134295 + keyFrame.GetHashCode();
            hashCode = hashCode * -1521134295 + map_fov_percentage.GetHashCode();
            hashCode = hashCode * -1521134295 + map_status.GetHashCode();
            hashCode = hashCode * -1521134295 + featurepoint.GetHashCode();
            hashCode = hashCode * -1521134295 + front_cam_points.GetHashCode();
            hashCode = hashCode * -1521134295 + map_id.GetHashCode();
            hashCode = hashCode * -1521134295 + map_number.GetHashCode();
            hashCode = hashCode * -1521134295 + current_height.GetHashCode();
            hashCode = hashCode * -1521134295 + original_height.GetHashCode();
            hashCode = hashCode * -1521134295 + lost_tracking_reason.GetHashCode();
            return hashCode;
        }
    }
}