using UnityEngine;
using System.Collections;
using Photon.Pun;
using DG.Tweening;
using HTC.UnityPlugin.Vive;

namespace AnchorSharing
{
    [RequireComponent(typeof(AudioSource))]
    public class GashaponController : MonoBehaviour
    {
        [SerializeField] private PhotonView photonView = null;
        [SerializeField] private Animator animator = null;
        [SerializeField] private Transform spawnPoint = null;
        [SerializeField] private RectTransform canvas = null;
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource = null;
        [SerializeField] private AudioClip audioClipSapwn = null;
        [SerializeField][Range(0.5f, 2f)] private float speed = 0;
        private bool bAllowThrowTheBall = false;
        private bool spawnInFixedRange = false;

        private const string Ani_Spin = "Spin";

        private void Start()
        {
            gameObject.transform.localScale = Vector3.zero;
            gameObject.transform.DOScale(Vector3.one, 1.25f).SetEase(Ease.OutElastic);

            float floatingAmplitude = 0.1f;
            float floatingDuration = Random.Range(1, 1.5f);
            canvas.DOLocalMoveY(canvas.transform.localPosition.y + floatingAmplitude, floatingDuration).SetLoops(-1, LoopType.Yoyo);
        }

        private void Update()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f &&
                animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == Ani_Spin &&
                bAllowThrowTheBall)
            {
                AudioPlay(audioClipSapwn);
                StartCoroutine(SpawnCapsule());
                bAllowThrowTheBall = false;
            }
        }

        private IEnumerator GashaponAnimation(int times, float sp)
        {
            speed = sp;
            while (times > 0)
            {
                times--;
                //Animator
                animator.speed = speed;
                animator.Play(Ani_Spin, 0, 0);
                animator.Update(0f);

                bAllowThrowTheBall = true;

                yield return new WaitForSeconds(1.5f / speed);
            }
        }

        private IEnumerator SpawnCapsule()
        {
            if (PhotonManager.Instance.IsMasterClient())
            {
                GameObject capsule = PhotonManager.Instance.InitiateObject(GameDefine.CAPSULE, spawnPoint.position, Quaternion.identity, 0, null);

                float degreeRange = 0;
                float radiusRange = 0;
                float heightRange = 0;
                if (!spawnInFixedRange)
                {
                    degreeRange = Random.Range(-50f, 50f);
                    radiusRange = Random.Range(1f, 3f);
                    heightRange = Random.Range(0.75f, 1.25f);
                }
                else if (spawnInFixedRange)
                {
                    degreeRange = Random.Range(-15f, 15f);
                    radiusRange = Random.Range(0.25f, 0.75f);
                    heightRange = Random.Range(0.75f, 1.25f);
                }

                Vector3 fwd = gameObject.transform.forward;
                Vector3 up = gameObject.transform.up;

                Quaternion rot = Quaternion.AngleAxis(degreeRange, up);
                Vector3 rotateVec = rot * fwd;
                Vector3 finalVec = rotateVec * radiusRange;
                finalVec = new Vector3(finalVec.x, heightRange, finalVec.z);

                Vector3 targetPos = gameObject.transform.position + finalVec;

                yield return new WaitUntil(() => capsule.activeSelf);
                yield return new WaitUntil(() => capsule.GetPhotonView() != null);

                capsule.GetPhotonView().RPC("AS_Capsule_Spawn",
                                            RpcTarget.All,
                                            targetPos
                                            );
                spawnInFixedRange = false;
            }
            yield return null;
        }

        private void AudioPlay(AudioClip clip)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == GameDefine.HAND_R_NAME || other.gameObject.name == GameDefine.HAND_L_NAME)
            {
                spawnInFixedRange = true;
                if (other.gameObject.name == GameDefine.HAND_R_NAME)
                    GameDefine.Haptic(HandRole.RightHand, 100);
                else if (other.gameObject.name == GameDefine.HAND_L_NAME)
                    GameDefine.Haptic(HandRole.LeftHand, 100);
                photonView.RPC("AS_Gashapon_SpawnCapsule",
                               RpcTarget.All,
                               1,
                               speed
                               );
            }
        }

        [PunRPC]
        private void AS_Gashapon_SpawnCapsule(int times, float sp)
        {
            StartCoroutine(GashaponAnimation(times, sp));
        }
    }
}