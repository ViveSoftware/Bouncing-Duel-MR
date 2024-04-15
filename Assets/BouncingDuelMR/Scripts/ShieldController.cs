using DG.Tweening;
using HTC.UnityPlugin.Vive;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnchorSharing
{
    public class ShieldController : MonoBehaviour, IPunObservable, IColorable
    {
        public List<Color32> Colors
        {
            set { colors = value; }
            get { return colors; }
        }
        [SerializeField] private List<Color32> colors = new List<Color32>();

        [SerializeField] private PhotonView photonView = null;
        [SerializeField] private Animator animator = null;

        [SerializeField] public GameObject[] bulletsInMagazine = new GameObject[9];

        [Header("Shiled Components")]
        [SerializeField] private GameObject loadedBullet = null;
        [SerializeField] private TMP_Text bulletCount = null;

        private VivePoseTracker[] vivePoseTrackers = null;
        private Transform leftHand = null;
        private Transform rightHand = null;

        private bool isLoaded = true;
        private bool isReloading = false;
        private bool sling = false;

        bool bullet0 { get { return bulletsInMagazine[0].activeSelf; } set { bulletsInMagazine[0].SetActive(value); } }
        bool bullet1 { get { return bulletsInMagazine[1].activeSelf; } set { bulletsInMagazine[1].SetActive(value); } }
        bool bullet2 { get { return bulletsInMagazine[2].activeSelf; } set { bulletsInMagazine[2].SetActive(value); } }
        bool bullet3 { get { return bulletsInMagazine[3].activeSelf; } set { bulletsInMagazine[3].SetActive(value); } }
        bool bullet4 { get { return bulletsInMagazine[4].activeSelf; } set { bulletsInMagazine[4].SetActive(value); } }
        bool bullet5 { get { return bulletsInMagazine[5].activeSelf; } set { bulletsInMagazine[5].SetActive(value); } }
        bool bullet6 { get { return bulletsInMagazine[6].activeSelf; } set { bulletsInMagazine[6].SetActive(value); } }
        bool bullet7 { get { return bulletsInMagazine[7].activeSelf; } set { bulletsInMagazine[7].SetActive(value); } }
        bool bullet8 { get { return bulletsInMagazine[8].activeSelf; } set { bulletsInMagazine[8].SetActive(value); } }

        private void Awake()
        {
            CheckIsGunLoaded();
            UpdateMagazineUI();
        }

        private IEnumerator Start()
        {
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(GameDefine.LAYER_BULLET),
                                         LayerMask.NameToLayer(GameDefine.LAYER_BULLET_IGNORE),
                                         true);

            yield return new WaitUntil(() => photonView.Owner.NickName != "");

            if (photonView.Owner.NickName == GameDefine.PLAYER_1_NAME)
                SetColor(colors[0]);
            else if (photonView.Owner.NickName == GameDefine.PLAYER_2_NAME)
                SetColor(colors[1]);

            vivePoseTrackers = FindObjectsOfType<VivePoseTracker>();
            foreach (var i in vivePoseTrackers)
            {
                if (i.viveRole == ViveRoleProperty.New(HandRole.RightHand) && i.gameObject.name == "RightHand")
                    rightHand = i.transform;
                else if (i.viveRole == ViveRoleProperty.New(HandRole.LeftHand) && i.gameObject.name == "LeftHand")
                    leftHand = i.transform;
            }
        }

        private void Update()
        {
            if (!photonView.IsMine || photonView.Owner.NickName == "")
                return;

            Fire();
        }
        
        public void SetColor(Color32 color)
        {
            loadedBullet.GetComponentInChildren<Renderer>().material.color = color;
            foreach (var i in bulletsInMagazine)
                i.GetComponentInChildren<Renderer>().material.color = color;
        }

        public void ResetGun()
        {
            sling = false;
            isLoaded = true;
            isReloading = false;
            bulletCount.text = "4";

            foreach (var i in bulletsInMagazine)
                i.SetActive(false);
            bulletsInMagazine[0].SetActive(true);
            bulletsInMagazine[1].SetActive(true);
            bulletsInMagazine[2].SetActive(true);
            bulletsInMagazine[3].SetActive(true);
            CheckIsGunLoaded();
        }

        public void ActiveBulletInMagazine(Vector3 relative)
        {
            foreach (var bullet in bulletsInMagazine)
            {
                if (!bullet.activeSelf)
                {
                    bullet.SetActive(true);
                    bullet.transform.localPosition = new Vector3(relative.x, relative.y, 0.05f);
                    if (!isLoaded && !isReloading)
                        StartCoroutine(ReloadNextRound());
                    UpdateMagazineUI();
                    return;
                }
                Debug.Log("[ShieldController][ActiveBulletInMagazine] Magazine full");
            }
        }

        public void DeactiveBulletInMagazine(int id)
        {
            if (bulletsInMagazine[id].activeSelf)
                bulletsInMagazine[id].SetActive(false);
            else
                Debug.LogError("[ShieldController][DeactiveBulletInMagazine] wrong status");

            UpdateMagazineUI();
        }

        private void Fire()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (leftHand == null || rightHand == null)
                return;
            Vector3 relative = transform.InverseTransformPoint(rightHand.position);
            if (relative.x < -0.075f && relative.x > 0.075f &&
                relative.y < 0.15f && relative.y > 0.25f &&
                relative.z < -0.35f && relative.z > 0f)
                return;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
            if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Trigger))
#elif UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Z))
#endif
            {
                if (isLoaded && gameObject.activeSelf && !sling)
                {
                    GameDefine.Haptic(HandRole.LeftHand, 100);
                    animator.ResetTrigger("UP_release");
                    animator.SetTrigger("UP_pull");
                    sling = true;
                }
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            else if (ViveInput.GetPressUp(HandRole.RightHand, ControllerButton.Trigger))
#elif UNITY_EDITOR
            else if (Input.GetKeyUp(KeyCode.Z))
#endif
            {
                if (isLoaded && gameObject.activeSelf && sling)
                {
                    GameDefine.Haptic(HandRole.RightHand, 100);
                    isLoaded = false;

                    animator.ResetTrigger("UP_pull");
                    animator.SetTrigger("UP_release");

                    float velocity = 5f;
                    object[] instantiationData = new object[] { BulletController.State.active, false, velocity };
                    PhotonManager.Instance.InitiateObject(GameDefine.BULLET, loadedBullet.transform.position, loadedBullet.transform.rotation, 0, instantiationData);
                    CheckIsGunLoaded();
                    StartCoroutine(ReloadNextRound());
                    sling = false;
                }
            }
        }

        private void CheckIsGunLoaded()
        {
            loadedBullet.SetActive(isLoaded);
            //loadedBullet.transform.localScale = Vector3.zero;
            //loadedBullet.transform.DOScale(new Vector3(0.01f, 0.01f, 0.01f), 0.5f).SetEase(Ease.InExpo);
        }

        private bool CheckIsBulletLeftInMagazine(out int index)
        {
            index = -1;
            for (int i = 0; i < bulletsInMagazine.Length; i++)
            {
                if (bulletsInMagazine[i].activeSelf)
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        private void UpdateMagazineUI()
        {
            int bulletLeft = 0;
            foreach (var bullet in bulletsInMagazine)
            {
                if (bullet.activeSelf)
                {
                    bulletLeft++;
                }
            }
            bulletCount.text = bulletLeft.ToString();
        }

        private IEnumerator ReloadNextRound()
        {
            Debug.Log("[ShieldController][ReloadNextRound]");
            if (isLoaded || !CheckIsBulletLeftInMagazine(out int index) || isReloading)
                yield break;
            Debug.Log("[ShieldController][ReloadNextRound] start");
            isReloading = true;
            yield return new WaitForSeconds(0.5f);

            isLoaded = true;
            CheckIsGunLoaded();
            photonView.RPC("AS_Player_DeactiveBulletInShield",
                           RpcTarget.All,
                           index
                           );
            isReloading = false;
        }

        #region IPunObservable implementation
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(this.isLoaded);
                stream.SendNext(bullet0);
                stream.SendNext(bullet1);
                stream.SendNext(bullet2);
                stream.SendNext(bullet3);
                stream.SendNext(bullet4);
                stream.SendNext(bullet5);
                stream.SendNext(bullet6);
                stream.SendNext(bullet7);
                stream.SendNext(bullet8);
            }
            else
            {
                this.isLoaded = (bool)stream.ReceiveNext();
                bullet0 = (bool)stream.ReceiveNext();
                bullet1 = (bool)stream.ReceiveNext();
                bullet2 = (bool)stream.ReceiveNext();
                bullet3 = (bool)stream.ReceiveNext();
                bullet4 = (bool)stream.ReceiveNext();
                bullet5 = (bool)stream.ReceiveNext();
                bullet6 = (bool)stream.ReceiveNext();
                bullet7 = (bool)stream.ReceiveNext();
                bullet8 = (bool)stream.ReceiveNext();

                CheckIsGunLoaded();
            }
        }
        #endregion
    }
}
