
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Native;

namespace AnchorSharing
{
    public class SceneMeshGenerationHelper
    {
        private class GenerationCommand
        {
            //vertices, indices, uv, tangent
            public Action<Vector3[], int[], Vector2[], Vector4[]> OnMeshGenerateCompleted;
            public WVR_SceneMesh SceneMesh;
        }

        private Queue<GenerationCommand> cmds = new Queue<GenerationCommand>();
        private Thread thread;
        private WVR_PoseOriginModel poseOriginModel;

        public void GenerateMesh(WVR_SceneMesh sceneMesh, Action<Vector3[], int[], Vector2[], Vector4[]> onMeshGenerateCompleted)
        {
            GenerationCommand cmd = new GenerationCommand()
            {
                SceneMesh = sceneMesh,
                OnMeshGenerateCompleted = onMeshGenerateCompleted,
            };

            cmds.Enqueue(cmd);

            if (thread == null || !thread.IsAlive)
            {
                poseOriginModel = ScenePerceptionManager.GetCurrentPoseOriginModel();
                thread = new Thread(() => generationProcess());
                thread.Name = "GenerateMeshThread";
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void generationProcess()
        {
            Debug.Log("SceneMeshGenerationHelper - thread start");
            while (cmds.Count > 0)
            {
                GenerationCommand cmd = cmds.Dequeue();

                WVR_Vector3f_t[] sceneVertexBuffer;
                uint[] sceneIndexBuffer;

                Debug.Log($"SceneMeshGenerationHelper - Start get scene mesh buffer for [{cmd.SceneMesh.meshBufferId}]");
                if (getSceneMeshBuffer(cmd.SceneMesh.meshBufferId, poseOriginModel, out sceneVertexBuffer, out sceneIndexBuffer) != WVR_Result.WVR_Success)
                {
                    Debug.Log("SceneMeshGenerationHelper - Failed to get scene mesh buffer.");
                    cmd.OnMeshGenerateCompleted.Invoke(null, null, null, null);
                    continue;
                }
                
                Vector3[] vertices;
                Vector2[] uvs;
                Vector4[] tangents;
                int[] indices;

                generateSceneMesh(sceneVertexBuffer, sceneIndexBuffer, out vertices, out indices, out uvs, out tangents);
                cmd.OnMeshGenerateCompleted.Invoke(vertices, indices, uvs, tangents);
            }
            Debug.Log("SceneMeshGenerationHelper - thread end");
        }

        private static WVR_Result getSceneMeshBuffer(UInt64 meshBufferId, WVR_PoseOriginModel poseOriginModel, out WVR_Vector3f_t[] vertexBuffer, out UInt32[] indexBuffer)
        {
            vertexBuffer = new WVR_Vector3f_t[0];
            indexBuffer = new UInt32[0];

            WVR_SceneMeshBuffer currentBuffer = new WVR_SceneMeshBuffer();

            currentBuffer.vertexCapacityInput = 0;
            currentBuffer.vertexCountOutput = 0;
            currentBuffer.vertexBuffer = IntPtr.Zero;
            currentBuffer.indexCapacityInput = 0;
            currentBuffer.indexCountOutput = 0;
            currentBuffer.indexBuffer = IntPtr.Zero;

            WVR_Result result = Interop.WVR_GetSceneMeshBuffer(meshBufferId, poseOriginModel, ref currentBuffer); //Get vertex and index count
            if (result != WVR_Result.WVR_Success)
            {
                Debug.Log("SceneMeshGenerationHelper.getSceneMeshBuffer - WVR_GetSceneMeshBuffer 1 failed with result " + result.ToString());
                return result;
            }
            else
            {
                currentBuffer.vertexCapacityInput = currentBuffer.vertexCountOutput;
                currentBuffer.indexCapacityInput = currentBuffer.indexCountOutput;

                Debug.Log("SceneMeshGenerationHelper.getSceneMeshBuffer - WVR_GetSceneMeshBuffer 1 Vertex Count Output: " + currentBuffer.vertexCapacityInput + ", Index Count Output: " + currentBuffer.indexCapacityInput);
            }

            WVR_Vector3f_t[] vertexBufferArray = new WVR_Vector3f_t[currentBuffer.vertexCapacityInput];
            UInt32[] indexBufferArray = new UInt32[currentBuffer.indexCapacityInput];

            WVR_Vector3f_t defaultWVRVector3f = default(WVR_Vector3f_t);
            currentBuffer.vertexBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(defaultWVRVector3f) * (int)currentBuffer.vertexCapacityInput);
            currentBuffer.indexBuffer = Marshal.AllocHGlobal(sizeof(UInt32) * (int)currentBuffer.indexCapacityInput);

            result = Interop.WVR_GetSceneMeshBuffer(meshBufferId, poseOriginModel, ref currentBuffer); //Get buffers
            if (result != WVR_Result.WVR_Success)
            {
                Debug.Log("SceneMeshGenerationHelper.getSceneMeshBuffer - WVR_GetSceneMeshBuffer 2 failed with result " + result.ToString());
                return result;
            }
            else
            {
                //Fill vertex buffer
                int offset = 0;
                for (int i = 0; i < currentBuffer.vertexCapacityInput; i++)
                {
                    if (IntPtr.Size == 4)
                        vertexBufferArray[i] = (WVR_Vector3f_t)Marshal.PtrToStructure(new IntPtr(currentBuffer.vertexBuffer.ToInt32() + offset), typeof(WVR_Vector3f_t));
                    else
                        vertexBufferArray[i] = (WVR_Vector3f_t)Marshal.PtrToStructure(new IntPtr(currentBuffer.vertexBuffer.ToInt64() + offset), typeof(WVR_Vector3f_t));

                    offset += Marshal.SizeOf(defaultWVRVector3f);
                }

                //Fill index buffer
                offset = 0;
                for (int i = 0; i < currentBuffer.indexCapacityInput; i++)
                {
                    if (IntPtr.Size == 4)
                        indexBufferArray[i] = (UInt32)Marshal.PtrToStructure(new IntPtr(currentBuffer.indexBuffer.ToInt32() + offset), typeof(UInt32));
                    else
                        indexBufferArray[i] = (UInt32)Marshal.PtrToStructure(new IntPtr(currentBuffer.indexBuffer.ToInt64() + offset), typeof(UInt32));

                    offset += sizeof(UInt32);
                }
            }

            vertexBuffer = vertexBufferArray;
            indexBuffer = indexBufferArray;

            Marshal.FreeHGlobal(currentBuffer.vertexBuffer);
            Marshal.FreeHGlobal(currentBuffer.indexBuffer);

            return result;
        }
        private void generateSceneMesh(WVR_Vector3f_t[] vertexBuffer, UInt32[] indexBuffer, out Vector3[] vertices, out int[] indices, out Vector2[] uvs, out Vector4[] tangents)
        {
            if(vertexBuffer.Length == 0 || indexBuffer.Length == 0)
            {
                Debug.LogError("Load empty scene meshes.");
                vertices = null; indices = null; uvs = null; tangents = null;
                return;
            }

            vertices = new Vector3[vertexBuffer.Length];
            for (int i = 0; i < vertexBuffer.Length; i++)
            {
                Coordinate.GetVectorFromGL(vertexBuffer[i], out vertices[i]);
                vertices[i] = vertices[i];
            }

            indices = new int[indexBuffer.Length];
            for (int i = 0; i < indexBuffer.Length; i += 3)
            {
                indices[i] = (int)indexBuffer[i];
                indices[i + 1] = (int)indexBuffer[i + 2];
                indices[i + 2] = (int)indexBuffer[i + 1];
            }

            uvs = new Vector2[vertexBuffer.Length];
            tangents = new Vector4[vertexBuffer.Length];
            Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
            for (int i = 0, y = 0; y < 2; y++)
            {
                for (int x = 0; x < 2; x++, i++)
                {
                    uvs[i] = new Vector2((float)x, (float)y);
                    tangents[i] = tangent;
                }
            }
        }
    }
}