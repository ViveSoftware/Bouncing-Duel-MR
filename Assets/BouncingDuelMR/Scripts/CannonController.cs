using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

namespace AnchorSharing
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class CannonController : MonoBehaviour, IColorable
    {
        [Header("Colors: Default->Colors[0], Player_1->Colors[1], Player_2->Colors[2]")]
        [SerializeField] private List<Color32> colors = new List<Color32>();
        public List<Color32> Colors
        {
            set { colors = value; }
            get { return colors; }
        }

        [Header("Components")]
        [SerializeField] private List<Renderer> renderers = new List<Renderer>();
        [SerializeField] private PhotonView photonView = null;
        [SerializeField] private Animator animator = null;
        [SerializeField] private RectTransform canvas = null;
        [SerializeField] private Slider slider = null;

        [Range(-3,3)]
        [SerializeField] private int value = 0;
        [SerializeField] private int minValue = -3;
        [SerializeField] private int maxValue = 3;

        [Header("Target")]
        [SerializeField] private Transform targetTransform = null;
        [SerializeField] private Transform pitchTransform = null;
        [SerializeField] private Transform yawTransform = null;
        [SerializeField] private Transform fireSpot = null;
        [SerializeField] private Transform fireSpotVFX = null;

        [SerializeField] private int maxPitchAngle = 60;
        [SerializeField] private int maxYawAngle = 180;
        [SerializeField] private int speed = 150;

        private bool allowFire = true;

        private const string Ani_Start = "Start";
        private const string Ani_Launch = "Launch";
        private const string Ani_End = "End";

        private void Start()
        {
            SetColor(colors[0]); 
            animator.Play(Ani_Start, 0, 0);
            animator.Update(0f);
            fireSpotVFX.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (GameManager.Instance.scoreBoardController != null)
            {
                if (!GameManager.Instance.scoreBoardController.IsGameRunning)
                {
                    StopAllCoroutines(); 
                    fireSpotVFX.gameObject.SetActive(false);
                    return;
                }
            }

            if (Camera.main != null)
            {
                canvas.LookAt(canvas.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
                Vector3 v3 = new Vector3(canvas.localEulerAngles.x, canvas.localEulerAngles.y, 0);
                canvas.localEulerAngles = v3;
            }

            if (GameManager.Instance.selfPlayerController != null && GameManager.Instance.otherPlayerController != null)
            {
                if (value >= minValue && value<0)
                {
                    if (PhotonManager.Instance.IsMasterClient())
                        targetTransform = GameManager.Instance.otherPlayerController.GetPlayerBody().transform;
                    else
                        targetTransform = GameManager.Instance.selfPlayerController.GetPlayerBody().transform;
                }
                else if (value <= maxValue && value>0)
                {
                    if (PhotonManager.Instance.IsMasterClient())
                        targetTransform = GameManager.Instance.selfPlayerController.GetPlayerBody().transform;
                    else
                        targetTransform = GameManager.Instance.otherPlayerController.GetPlayerBody().transform;
                }
                else
                {
                    StopAllCoroutines(); 
                    fireSpotVFX.gameObject.SetActive(false);
                    targetTransform = null;
                }

                if (targetTransform != null)
                {
                    if (pitchTransform == yawTransform)
                        pitchTransform.localEulerAngles = new Vector3(PitchRotation(speed * Time.deltaTime), YawRotation(speed * Time.deltaTime), 0);
                    else
                    {
                        pitchTransform.localEulerAngles = new Vector3(PitchRotation(speed * Time.deltaTime), 0, 0);
                        yawTransform.localEulerAngles = new Vector3(0, YawRotation(speed * Time.deltaTime), 0);
                    }

                    float aimAngleOffset = Vector3.Angle(fireSpot.forward, targetTransform.position - fireSpot.position);
                    if (aimAngleOffset < 5)
                    {
                        Debug.DrawRay(fireSpot.position, fireSpot.forward * 100, Color.green);
                        if (allowFire)
                        {
                            StartCoroutine(Fire());
                            allowFire = false;
                        }
                    }
                    else
                    {
                        Debug.DrawRay(fireSpot.position, fireSpot.forward * 100, Color.red);
                        StopAllCoroutines(); 
                        fireSpotVFX.gameObject.SetActive(false);
                        allowFire = true;
                    }
                }
            }
        }

        public void SetColor(Color32 color)
        {
            foreach (var i in renderers)
                foreach (var j in i.materials)
                {
                    j.SetColor("_ColorA", color);
                    j.SetColor("_ColorB", color);
                }
        }

        private Color[] GetInterpolatedColors(Color colorA, Color colorB)
        {
            Color[] interpolatedColors = new Color[4];

            float step = 1f / 3f; // Divide the range into 5 steps

            for (int i = 0; i < 4; i++)
            {
                float t = step * i;
                interpolatedColors[i] = Color.Lerp(colorA, colorB, t);
            }

            return interpolatedColors;
        }

        private float PitchRotation(float speed)
        {
            float mCurrentPitchAngle = pitchTransform.localEulerAngles.x;
            if (mCurrentPitchAngle > 180)
                mCurrentPitchAngle = -(360 - mCurrentPitchAngle);

            Vector3 mPitchTarget = Vector3.ProjectOnPlane(targetTransform.position - fireSpot.localPosition - pitchTransform.position, pitchTransform.right).normalized;
            float mPitchAngleOffset = Vector3.Angle(pitchTransform.forward, mPitchTarget);

            if (mPitchAngleOffset == 0)
                return mCurrentPitchAngle;

            if (mPitchAngleOffset < speed)
                speed = mPitchAngleOffset;

            Vector3 mPitchCross = Vector3.Cross(mPitchTarget, pitchTransform.forward).normalized;
            if (mCurrentPitchAngle > -maxPitchAngle && mPitchCross == pitchTransform.right)
                return mCurrentPitchAngle - speed;
            else if (mCurrentPitchAngle < maxPitchAngle && mPitchCross != pitchTransform.right)
                return mCurrentPitchAngle + speed;
            return mCurrentPitchAngle;
        }

        private float YawRotation(float speed)
        {
            float mCurrentYawAngle = yawTransform.localEulerAngles.y;
            if (mCurrentYawAngle > 180)
                mCurrentYawAngle = -(360 - mCurrentYawAngle);

            Vector3 mYawTarget = Vector3.ProjectOnPlane(targetTransform.position - yawTransform.position, yawTransform.up).normalized;
            float mYawAngleOffset = Vector3.Angle(yawTransform.forward, mYawTarget);

            if (mYawAngleOffset == 0)
                return mCurrentYawAngle;

            if (mYawAngleOffset < speed)
                speed = mYawAngleOffset;

            Vector3 mYawCross = Vector3.Cross(mYawTarget, yawTransform.forward).normalized;
            if (mCurrentYawAngle > -maxYawAngle && mYawCross == yawTransform.up)
                return mCurrentYawAngle - speed;
            else if (mCurrentYawAngle < maxYawAngle && mYawCross != yawTransform.up)
                return mCurrentYawAngle + speed;
            return mCurrentYawAngle;
        }

        private IEnumerator Fire()
        {
            while (true)
            {
                if (photonView.IsMine)
                {
                    float velocity = 3f;
                    object[] instantiationData = new object[] { BulletController.State.active, true, velocity };
                    PhotonManager.Instance.InitiateObject(GameDefine.BULLET, fireSpot.position, fireSpot.rotation, 0, instantiationData);
                }
                fireSpotVFX.gameObject.SetActive(true);

                animator.Play(Ani_Launch, 0, 0);
                animator.Update(0f);
                yield return new WaitForSeconds(1);

                fireSpotVFX.gameObject.SetActive(false);
            }
        }

        [PunRPC]
        private void AS_Cannon_UpdateValue(string ptvName)
        {
            if (ptvName == GameDefine.PLAYER_1_NAME)
                value = value - 1;
            else if (ptvName == GameDefine.PLAYER_2_NAME)
                value = value + 1;
            value = Mathf.Clamp(value, minValue, maxValue);
            slider.value = value;

            if (value >= minValue && value < 0)
                photonView.TransferOwnership(PhotonNetwork.PlayerList[0]);
            if (value <= maxValue && value > 0)
                photonView.TransferOwnership(PhotonNetwork.PlayerList[1]);
            
            Color[] interpolatedColors = GetInterpolatedColors(colors[0], colors[0]);
            if (value < 0)
                interpolatedColors = GetInterpolatedColors(colors[0], colors[1]);
            else if (value > 0)
                interpolatedColors = GetInterpolatedColors(colors[0], colors[2]);

            if (value != 0)
                SetColor(interpolatedColors[Mathf.Abs(value)]);
            else
                SetColor(interpolatedColors[0]);
        }
    }
}