using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingFlash : MonoBehaviour
{
   public float emissionSpeed = 0.5f;
   private Material material;
   private float currentEmissionStrength = 1f;
   void Start()
   {
       Renderer renderer = GetComponent<Renderer>();
       material = renderer.material;
   }
   void Update()
   {
       if (Input.GetKeyDown(KeyCode.Space))
       {
           currentEmissionStrength = 1f;
       }

       currentEmissionStrength -= emissionSpeed * Time.deltaTime;
       currentEmissionStrength = Mathf.Clamp01(currentEmissionStrength);

       material.SetFloat("_EmissionStrength", currentEmissionStrength);
   }
}