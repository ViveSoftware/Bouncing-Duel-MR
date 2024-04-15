using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wave.Native;

namespace Wave.Essence.TrackableMarker.Sample
{
	public class MarkerPrefab : MonoBehaviour
	{
		public GameObject markerAreaGO = null;
		public Material markerAreaMatTemplate = null;

		public Color detectedStateColor = Color.blue;
		public Color trackedStateColor = Color.green;
		public Color pausedStateColor = Color.yellow;
		public Color stoppedStateColor = Color.red;

		public Toggle trackableToggle = null;
		public Canvas trackableToggleCanvas = null;
		public Text markerInfoText = null;

		private Material markerAreaMatInstance = null;
		private Mesh markerAreaGeneratedMesh = null;

		public BaseMarker markerRef = null;

		private const string LOG_TAG = "MarkerPrefab";

		public void InitMarkerPrefab()
		{
			if (markerAreaMatTemplate != null)
			{
				markerAreaMatInstance = new Material(markerAreaMatTemplate);
				markerAreaGO.GetComponent<MeshRenderer>().sharedMaterial = markerAreaMatInstance;

				AssignMarkerAreaMesh(markerAreaGO.GetComponent<MeshFilter>(), markerAreaGO.GetComponent<MeshCollider>());
			}

			//HideTracakbleToggle();
			trackableToggleCanvas.worldCamera = Camera.main;
		}

		private void OnDestroy()
		{
			if (markerAreaGO != null)
			{
				if (markerAreaGeneratedMesh != null)
				{
					Destroy(markerAreaGeneratedMesh);
				}

				Destroy(markerAreaGO);
				markerAreaGO = null;
			}
		}

		private void Update()
		{
			trackableToggleCanvas.gameObject.transform.forward = Camera.main.transform.forward;

			UpdateMarkerPrefabInfoText();
		}

		public void UpdateMarkerPrefabInfoText()
		{
			switch(markerRef.target)
			{
				case WVR_MarkerObserverTarget.WVR_MarkerObserverTarget_Aruco:
					{
						ArucoMarker arucoMarkerRef = (ArucoMarker)markerRef;

						markerInfoText.text = "Target: " + "Aruco" + "\n"
											+ "TrackerID: " + arucoMarkerRef.trackerId.ToString() + "\n"
											+ "Size: " + arucoMarkerRef.size.ToString() + "\n"
											+ arucoMarkerRef.state.ToString() + "\n";

						if (arucoMarkerRef.markerName.name != null)
						{
							markerInfoText.text += new string(arucoMarkerRef.markerName.name) + "\n";
						}

						break;
					}
				default:
					markerInfoText.text = "Placeholder text";
					break;
			}
		}

		public void ShowTracakbleToggle()
		{
			trackableToggle.gameObject.SetActive(true);
		}

		public void HideTracakbleToggle()
		{
			trackableToggle.gameObject.SetActive(false);
		}

		public void ToggleMarkerTrackable(bool trackable)
		{
			if (trackable)
			{
				MarkerPrefabEventMediator.OnClickTrackMarkerEvent?.Invoke(this);
			}
			else
			{
				MarkerPrefabEventMediator.OnClickUntrackMarkerEvent?.Invoke(this);
			}
		}

		public void UpdateMarkerPrefabSize(float size)
		{
			if (markerAreaGO != null)
			{
				markerAreaGO.transform.localScale = new Vector3(size, size, size);
			}
		}

		public void UpdateMarkerPrefabPose(Vector3 position, Quaternion rotation)
		{
			transform.position = position;
			transform.rotation = rotation;
		}

		public void UpdateMarkerPrefabTrackingState(WVR_MarkerTrackingState markerTrackingState)
		{	
			switch (markerTrackingState)
			{
				case WVR_MarkerTrackingState.WVR_MarkerTrackingState_Detected:
					{
						if (markerAreaMatInstance != null) markerAreaMatInstance.SetColor("_Color", detectedStateColor);
						if (trackableToggle.isOn) trackableToggle.SetIsOnWithoutNotify(false);
						break;
					}
				case WVR_MarkerTrackingState.WVR_MarkerTrackingState_Paused:
					{
						if (markerAreaMatInstance != null) markerAreaMatInstance.SetColor("_Color", pausedStateColor);
						if (!trackableToggle.isOn) trackableToggle.SetIsOnWithoutNotify(true);
						break;
					}
				case WVR_MarkerTrackingState.WVR_MarkerTrackingState_Stopped:
					{
						if (markerAreaMatInstance != null) markerAreaMatInstance.SetColor("_Color", stoppedStateColor);
						if (!trackableToggle.isOn) trackableToggle.SetIsOnWithoutNotify(true);
						break;
					}
				case WVR_MarkerTrackingState.WVR_MarkerTrackingState_Tracked:
					{
						if (markerAreaMatInstance != null) markerAreaMatInstance.SetColor("_Color", trackedStateColor);
						if (!trackableToggle.isOn) trackableToggle.SetIsOnWithoutNotify(true);
						break;
					}
				default:
					markerAreaMatInstance.SetColor("_Color", Color.white);
					break;
			}
		}

		private void AssignMarkerAreaMesh(MeshFilter markerAreaMeshFilter, MeshCollider markerAreaMeshCollider)
		{
			markerAreaGeneratedMesh  = GenerateMarkerSquareMesh(1f); //Generate 1x1 m^2 mesh, then marker size as scale instead
			markerAreaMeshFilter.sharedMesh = markerAreaGeneratedMesh;
			markerAreaMeshCollider.sharedMesh = markerAreaGeneratedMesh;
		}

		private Mesh GenerateMarkerSquareMesh(float markerSize)
		{
			if (markerSize < 0) return null;

			WVR_Extent2Df markerExtents = new WVR_Extent2Df();
			markerExtents.width = markerExtents.height = markerSize;

			return GenerateQuadMesh(GenerateQuadVertex(markerExtents));
		}

		private Vector3[] GenerateQuadVertex(WVR_Extent2Df extend2D)
		{
			Vector3[] vertices = new Vector3[4]; //Four corners

			vertices[0] = new Vector3(-extend2D.width / 2, -extend2D.height / 2, 0); //Bottom Left
			vertices[1] = new Vector3(extend2D.width / 2, -extend2D.height / 2, 0); //Bottom Right
			vertices[2] = new Vector3(-extend2D.width / 2, extend2D.height / 2, 0); //Top Left
			vertices[3] = new Vector3(extend2D.width / 2, extend2D.height / 2, 0); //Top Right

			return vertices;
		}

		private Mesh GenerateQuadMesh(Vector3[] vertices)
		{
			Mesh quadMesh = new Mesh();
			quadMesh.vertices = vertices;

			//Create array that represents vertices of the triangles
			int[] triangles = new int[6];
			triangles[0] = 0;
			triangles[1] = 1;
			triangles[2] = 2;

			triangles[3] = 1;
			triangles[4] = 3;
			triangles[5] = 2;

			quadMesh.triangles = triangles;
			Vector2[] uv = new Vector2[vertices.Length];
			Vector4[] tangents = new Vector4[vertices.Length];
			Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
			for (int i = 0, y = 0; y < 2; y++)
			{
				for (int x = 0; x < 2; x++, i++)
				{
					uv[i] = new Vector2((float)x, (float)y);
					tangents[i] = tangent;
				}
			}
			quadMesh.uv = uv;
			quadMesh.tangents = tangents;

			return quadMesh;
		}
	}
}
