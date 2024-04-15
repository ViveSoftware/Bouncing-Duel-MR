using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;

namespace AnchorSharing
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class ReboundController : MonoBehaviour, IColorable
    {
        public enum Type
        {
            Acceleration = 0,
            Deceleration = 1,
            SpecifiedObject = 2
        }

        [Header("Colors")]
        [SerializeField] private Type type = Type.Acceleration;
        [SerializeField] private List<Color32> colors = new List<Color32>();
        public List<Color32> Colors
        {
            set { colors = value; }
            get { return colors; }
        }

        [Header("Components")]
        [SerializeField] private List<GameObject> objectType = new List<GameObject>();
        [SerializeField] private PhotonView photonView = null;
        [SerializeField] private AudioSource audioSource = null;
        [SerializeField] private AudioClip audioClipShake = null;

        [Header("Values")]
        [SerializeField] private float shakeDuration = 1;
        [SerializeField] private Vector3 shakeDiration = Vector3.one;
        [SerializeField] private float shakeStrength = 0.05f;
        [SerializeField] private int shakeVibrato = 50;

        private void Start()
        {
            gameObject.transform.localScale = Vector3.zero;
            gameObject.transform.DOScale(Vector3.one, Random.Range(1f, 1.25f)).SetEase(Ease.OutElastic);
        }

        public void SetColor(Color32 color)
        {
        }
        
        public Type GetRebounderType()
        {
            return type;
        }

        /// <summary>
        /// Photon RPC Functions
        /// </summary>
        [PunRPC]
        private void AS_Rebounder_Hit(float strength, Vector3 diration)
        {
            //strength 0.01 ~ 0.05
            shakeDiration = diration;
            shakeStrength = strength;

            //transform.DOShakePosition(shakeDuration, shakeDiration * shakeStrength, shakeVibrato);
            foreach (var i in objectType)
            {
                if (i.gameObject.activeSelf)
                    i.GetComponent<Animator>().SetTrigger("hit");
            }

            audioSource.Stop();
            audioSource.clip = audioClipShake; 
            audioSource.Play();
        }

        [PunRPC]
        private void AS_Rebounder_ChangeType(Type t, int index)
        {
            System.Array values = System.Enum.GetValues(typeof(Type));
            if (colors.Count < values.Length)
            {
                Debug.LogWarning($"Colors list count is not enough!");
                return;
            }

            if (index != -1)
            {
                foreach(var i in objectType)
                    i.gameObject.SetActive(false);
                objectType[index].gameObject.SetActive(true);
            }

            type = t;
            switch (type)
            {
                case Type.Acceleration:
                    SetColor(Colors[0]);
                    break;
                case Type.Deceleration:
                    SetColor(Colors[1]);
                    break;
                case Type.SpecifiedObject:
                    SetColor(Colors[2]);
                    break;
            }
            //SetColor(Colors[(int)t]);
        }
    }
}