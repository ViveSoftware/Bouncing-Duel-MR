using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxingGloveAniCtrl : MonoBehaviour
{

    private Animator animator;
    public float FistValue;
    

    void Start()
    {
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        FistValue = Mathf.Clamp(FistValue,0f,1f);
        animator.SetFloat("fist",FistValue);

    }
}
