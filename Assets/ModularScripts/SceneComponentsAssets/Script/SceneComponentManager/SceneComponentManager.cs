
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wave.Native;

namespace AnchorSharing
{
    public class SceneComponentManager : MonoBehaviour
    {
        /// <summary>
        /// Reterieve scene component objects
        /// </summary>
        public static Func<List<SceneComponentBase>> GetAllSceneComponents;
        /// <summary>
        /// Reterieve scene meshes
        /// </summary>
        public static Func<List<SceneMeshBase>> GetSceneColliderMesh;
        public static Func<List<SceneMeshBase>> GetSceneVisualMesh;

        [SerializeField] private SceneComponentDefine[] componentPrefabMap;
        [SerializeField] private SceneMeshBase colliderMeshPrefab;
        [SerializeField] private SceneMeshBase visualMeshPrefab;

        private Dictionary<WVR_ScenePlaneLabel, SceneComponentBase> sceneComponentPrefabMap;
        private List<SceneComponentBase> sceneComponents;
        private List<SceneMeshBase> colliderMeshes;
        private List<SceneMeshBase> visualMeshes;

        private void Awake()
        {
            sceneComponentPrefabMap = new Dictionary<WVR_ScenePlaneLabel, SceneComponentBase>();
            foreach(SceneComponentDefine scenePlaneDefine in componentPrefabMap)
            {
                sceneComponentPrefabMap[scenePlaneDefine.Label] = scenePlaneDefine.Prefab;
            }

            sceneComponents = new List<SceneComponentBase>();
            colliderMeshes = new List<SceneMeshBase>();
            visualMeshes = new List<SceneMeshBase>();

            GetAllSceneComponents += () => { return sceneComponents; };
            GetSceneColliderMesh += () => { return colliderMeshes; };
            GetSceneVisualMesh += () => { return visualMeshes; };
        }

        private void OnDestroy()
        {
            GetAllSceneComponents = null;
            GetSceneColliderMesh = null;
            GetSceneVisualMesh = null;
        }

        public void Clear()
        {
            foreach (SceneComponentBase sceneComp in sceneComponents) Destroy(sceneComp.gameObject);
            foreach (SceneMeshBase sceneMesh in colliderMeshes) Destroy(sceneMesh.gameObject);
            foreach (SceneMeshBase sceneMesh in visualMeshes) Destroy(sceneMesh.gameObject);
            sceneComponents.Clear();
            colliderMeshes.Clear();
        }

        public void GenerateSceneMeshes(SceneMeshData[] colliderMeshData, SceneMeshData[] visualMeshData)
        {
            if(colliderMeshData != null)
            {
                for (int i = 0; i < colliderMeshData.Length; i++)
                {
                    SceneMeshBase sceneMesh = Instantiate(colliderMeshPrefab, transform);
                    sceneMesh.SetData(colliderMeshData[i]);
                    colliderMeshes.Add(sceneMesh);
                }
            }

            if (visualMeshData != null)
            {
                for (int i = 0; i < visualMeshData.Length; i++)
                {
                    SceneMeshBase sceneMesh = Instantiate(visualMeshPrefab, transform);
                    sceneMesh.SetData(visualMeshData[i]);
                    visualMeshes.Add(sceneMesh);
                }
            }
        }

        public void UpdateSceneMeshes(SceneMeshData[] colliderMeshData, SceneMeshData[] visualMeshData)
        {
            foreach (SceneMeshBase sceneMesh in colliderMeshes) Destroy(sceneMesh.gameObject);
            colliderMeshes.Clear();
            foreach (SceneMeshBase sceneMesh in visualMeshes) Destroy(sceneMesh.gameObject);
            visualMeshes.Clear();

            GenerateSceneMeshes(colliderMeshData, visualMeshData);
        }

        public void GenerateScenePlanes(ScenePlaneData[] planeDatas)
        {
            foreach(ScenePlaneData planeData in planeDatas)
            {
                generateScenePlane(planeData);
            }
        }

        public void UpdateScenePlanes(ScenePlaneData[] planeDatas)
        {
            List<int> removeIndices = new List<int>();
            HashSet<int> addIndices = new HashSet<int>();
            
            for(int i=0; i<planeDatas.Length; i++)
            {
                addIndices.Add(i);
            }

            for(int i=0; i<sceneComponents.Count; i++)
            {
                bool shouldBeRemoved = true;
                for(int j=0; j<planeDatas.Length; ++j)
                {
                    if (sceneComponents[i].Data == planeDatas[j])
                    {
                        sceneComponents[i].UpdateData(planeDatas[j]);
                        addIndices.Remove(j);
                        shouldBeRemoved = false;
                        break;
                    }
                }

                if(shouldBeRemoved)
                {
                    removeIndices.Add(i);
                }
            }

            for(int i=removeIndices.Count-1; i>=0; i--)
            {
                Destroy(sceneComponents[removeIndices[i]]);
                sceneComponents.RemoveAt(i);
            }

            foreach(int newPlaneIndex in addIndices)
            {
                generateScenePlane(planeDatas[newPlaneIndex]);
            }
        }

        private void generateScenePlane(ScenePlaneData planeData)
        {
            if (sceneComponentPrefabMap.TryGetValue(planeData.sceneLabel, out SceneComponentBase prefab))
            {
                SceneComponentBase sceneComponent = Instantiate(prefab, transform);               
                sceneComponent.SetData(planeData);
                sceneComponents.Add(sceneComponent);
            }
            else
            {
                Debug.LogWarning($"Plane type [{planeData.sceneLabel} has no prefab]");
            }
        }
    }

    [Serializable]
    public struct SceneComponentDefine
    {
        public WVR_ScenePlaneLabel Label;
        public SceneComponentBase Prefab;
    }

}