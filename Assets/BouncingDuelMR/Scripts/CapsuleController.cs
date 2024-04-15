using UnityEngine;
using DG.Tweening;
using Photon.Pun;

namespace AnchorSharing
{
    [RequireComponent(typeof(AudioSource))]
    public class CapsuleController : MonoBehaviour
    {
        [SerializeField] private PhotonView photonView = null;
        [SerializeField] private Animator animator = null;
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource = null;
        [SerializeField] private AudioClip audioClipOpen = null;
        [SerializeField][Range(1, 2f)] public float moveingDuration = 1;
        [SerializeField][Range(0, 1f)] public float floatingAmplitude = 0.1f;
        [SerializeField][Range(1, 2f)] public float floatingDuration = 1;
        [SerializeField] private GameObject trail = null;

        private bool isOpen = false;
        private bool bOpenAni = false;
        private bool bEndAni = false;

        private bool isSelfPlayerGetReward = false;
        private Vector3 getRewardPlayerPos = Vector3.zero;

        private const string Ani_Capsule_Open = "Capsule_Open";

        private void Start()
        {

        }

        private void Update()
        {
            if (!isOpen && Vector3.Distance(gameObject.transform.position, GameManager.Instance.selfPlayerController.GetPlayerBody().transform.position) < 1f)
            {
                isOpen = true;
                isSelfPlayerGetReward = true;
                Vector3 getCapsulePlayerPos = GameManager.Instance.selfPlayerController.GetPlayerShield().transform.position;
                photonView.RPC("AS_Capsule_Open",
                               RpcTarget.All,
                               getCapsulePlayerPos
                               );
            }
            CheckAnimation();
        }

        private void CheckAnimation()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f &&
                animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == Ani_Capsule_Open &&
                !bOpenAni)
            {
                AudioPlay(audioClipOpen);
                bOpenAni = true;
                trail.SetActive(true);
                trail.transform.DOMove(getRewardPlayerPos, 0.2f);
            }
            else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f &&
                     animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == Ani_Capsule_Open &&
                     !bEndAni)
            {
                bEndAni = true;
                if (isSelfPlayerGetReward)
                    GameManager.Instance.selfPlayerController.AddBullets(Random.Range(3, 5));
                if (PhotonManager.Instance.IsMasterClient())
                    PhotonManager.Instance.DestoryObject(gameObject);
            }
        }

        private void AudioPlay(AudioClip clip)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }

        [PunRPC]
        private void AS_Capsule_Open(Vector3 pos)
        {
            if (!isOpen) isOpen = true;
            getRewardPlayerPos = pos;
            animator.Play(Ani_Capsule_Open, 0, 0);
            animator.Update(0f);
        }

        [PunRPC]
        private void AS_Capsule_Spawn(Vector3 targetPos)
        {
            gameObject.transform.DOMove(targetPos, moveingDuration).SetEase(Ease.OutBack).OnComplete(() =>
            {
                gameObject.transform.DOMoveY(targetPos.y + floatingAmplitude, floatingDuration).SetLoops(-1, LoopType.Yoyo);
            });
        }
    }
}