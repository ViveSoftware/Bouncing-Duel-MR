using DG.Tweening;
using HTC.UnityPlugin.Vive;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnchorSharing
{
    public class GunController : MonoBehaviour, IPunObservable, IColorable
    {
        public List<Color32> Colors
        {
            set { colors = value; }
            get { return colors; }
        }
        [SerializeField] private List<Color32> colors = new List<Color32>();

        [SerializeField] private PhotonView photonView = null;
        [SerializeField] public GameObject[] bulletsInMagazine = new GameObject[9];

        [Header("Gun Components")]
        [SerializeField] private GameObject loadedBullet = null;
        [SerializeField] private Renderer rendererGun = null;
        [SerializeField] private ParticleSystem vfxFire= null;
        [SerializeField] private TMP_Text bulletCount = null;

        [Header("Glove Components")]
        [SerializeField] private GameObject glove = null;
        [SerializeField] private Animator animatorGlove = null;

        private bool isLoaded = true;
        private bool isReloading = false;
        private float currentEmissionStrength = 1f;
        
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
            yield return new WaitUntil(() => photonView.Owner.NickName != "");

            if (photonView.Owner.NickName == GameDefine.PLAYER_1_NAME)
                SetColor(colors[0]);
            else if (photonView.Owner.NickName == GameDefine.PLAYER_2_NAME)
                SetColor(colors[1]);
        }

        private void Update()
        {
            currentEmissionStrength -= 10 * Time.deltaTime;
            currentEmissionStrength = Mathf.Clamp01(currentEmissionStrength);
            rendererGun.material.SetFloat("_EmissionStrength", currentEmissionStrength);

            if (!photonView.IsMine || photonView.Owner.NickName == "")
                return;
            
            Fire();
            Defense();
        }

        public void SetColor(Color32 color)
        {
            loadedBullet.GetComponentInChildren<Renderer>().material.color = color;
            foreach (var i in bulletsInMagazine)
                i.GetComponentInChildren<Renderer>().material.color = color;
        }

        public void ResetGun()
        {
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
                    bullet.transform.localPosition = new Vector3(-0.06f, relative.y, relative.z);
                    if (!isLoaded && !isReloading) 
                        StartCoroutine(ReloadNextRound());
                    UpdateMagazineUI();
                    return;
                }
                Debug.Log("[GunController][ActiveBulletInMagazine] Magazine full");
            }
        }

        public void DeactiveBulletInMagazine(int id)
        {
            if (bulletsInMagazine[id].activeSelf) 
                bulletsInMagazine[id].SetActive(false);
            else 
                Debug.LogError("[GunController][DeactiveBulletInMagazine] wrong status");

            UpdateMagazineUI();
        }

        private void Fire()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (ViveInput.GetPressUp(HandRole.RightHand, ControllerButton.Trigger))
#elif UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Z))
#endif
            {
                if (isLoaded && gameObject.activeSelf)
                {
                    vfxFire.Play();
                    currentEmissionStrength = 1f;
                    GameDefine.Haptic(HandRole.RightHand, 100);
                    isLoaded = false;
                    float velocity = 3f;
                    object[] instantiationData = new object[] { BulletController.State.active, false, velocity };
                    PhotonManager.Instance.InitiateObject(GameDefine.BULLET, loadedBullet.transform.position, loadedBullet.transform.rotation, 0, instantiationData);
                    CheckIsGunLoaded();
                    StartCoroutine(ReloadNextRound());
                }
            }
        }

        private void Defense()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Trigger))
#elif UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.G))
#endif
            {
                if (glove.activeSelf)
                    animatorGlove.SetFloat("fist", 1);
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            else if (ViveInput.GetPressUp(HandRole.LeftHand, ControllerButton.Trigger))
#elif UNITY_EDITOR
            else if (Input.GetKeyUp(KeyCode.G))
#endif
            {
                if (glove.activeSelf)
                    animatorGlove.SetFloat("fist", 0);
            }
        }

        private void CheckIsGunLoaded()
        {
            loadedBullet.SetActive(isLoaded);
            loadedBullet.transform.localScale = Vector3.zero;
            loadedBullet.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InExpo);
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
            Debug.Log("[GunController][ReloadNextRound]");
            if (isLoaded || !CheckIsBulletLeftInMagazine(out int index) || isReloading) 
                yield break;
            Debug.Log("[GunController][ReloadNextRound] start");
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
                // We own this player: send the others our data
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
                // Network player, receive data
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
