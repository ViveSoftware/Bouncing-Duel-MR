using UnityEngine;
using Photon.Pun;
using HTC.UnityPlugin.Vive;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

namespace AnchorSharing
{
    public sealed class PlayerController : MonoBehaviour, IColorable
    {
        [Header("Colors: Player_1->Colors[0], Player_2->Colors[1]")]
        [SerializeField] private List<Color32> colors = new List<Color32>();
        public List<Color32> Colors
        {
            set { colors = value; }
            get { return colors; }
        }

        public enum WeaponType
        {
            GunShield, GunGlove, ShieldV2
        }
        public WeaponType weaponType = WeaponType.GunShield;

        [Header("Components")]
        [SerializeField] private List<Renderer> renderers = new List<Renderer>();
        [SerializeField] private List<Renderer> ignoreRenderers = new List<Renderer>();
        [SerializeField] private PhotonView photonView = null;
        [SerializeField] private PhotonTransformView ptvHead = null;
        [SerializeField] private PhotonTransformView ptvHandL = null;
        [SerializeField] private PhotonTransformView ptvHandR = null;

        [Header("Injured Effect")]
        [SerializeField] private MeshRenderer injuredRenderer = null;
        [SerializeField] private Texture2D injuredTex = null;
        [SerializeField] private Texture2D injuredTexDir = null;

        [Header("Body Parts & Equipments")]
        [SerializeField] private GameObject body = null;
        [SerializeField] private GameObject gun = null;
        [SerializeField] private GameObject glove = null;
        [SerializeField] private GameObject shield = null;
        [SerializeField] private GameObject shield2 = null;

        private VRCameraHook cameraHook = null;
        private VivePoseTracker[] vivePoseTrackers = null;
        private Transform leftHand = null;
        private Transform rightHand = null;

        private void OnEnable()
        {
            GameManager.Instance.OnGameStart.AddListener(CB_OpenEquipment);
            GameManager.Instance.OnGameEnd.AddListener(CB_CloseEquipment);
        }

        private void OnDisable()
        {
            GameManager.Instance.OnGameStart.RemoveListener(CB_OpenEquipment);
            GameManager.Instance.OnGameEnd.RemoveListener(CB_CloseEquipment);
        }

        private IEnumerator Start()
        {
            yield return Init();

            if (!photonView.IsMine)
                yield break;

            foreach (var igr in ignoreRenderers)
                igr.gameObject.layer = LayerMask.NameToLayer(GameDefine.LAYER_CAMERA_IGNORE);

            cameraHook = FindObjectOfType<VRCameraHook>();
            vivePoseTrackers = FindObjectsOfType<VivePoseTracker>();
            foreach (var i in vivePoseTrackers)
            {
                if (i.viveRole == ViveRoleProperty.New(HandRole.RightHand) && i.gameObject.name == "RightHand")
                    rightHand = i.transform;
                else if (i.viveRole == ViveRoleProperty.New(HandRole.LeftHand) && i.gameObject.name == "LeftHand")
                    leftHand = i.transform;
            }
        }

        private IEnumerator Init()
        {
            yield return new WaitUntil(() => photonView.Owner.NickName != "");

            if (photonView.Owner.NickName == GameDefine.PLAYER_1_NAME)
                SetColor(colors[0]);
            else if (photonView.Owner.NickName == GameDefine.PLAYER_2_NAME)
                SetColor(colors[1]);

            object[] instantiationData = photonView.InstantiationData;
            weaponType = (WeaponType)instantiationData[0];

            //Avoid the default bullet gray
            yield return new WaitForEndOfFrame();
            CB_CloseEquipment();
        }

        private void Update()
        {
            if (!photonView.IsMine || photonView.Owner.NickName == "")
                return;

            if (cameraHook == null || leftHand == null || rightHand == null)
                return;

            ptvHead.transform.position = cameraHook.transform.position;
            ptvHead.transform.rotation = cameraHook.transform.rotation;

            ptvHandL.transform.position = leftHand.transform.position;
            ptvHandL.transform.rotation = leftHand.transform.rotation;

            ptvHandR.transform.position = rightHand.transform.position;
            ptvHandR.transform.rotation = rightHand.transform.rotation;
        }

        public void SetColor(Color32 color)
        {
            foreach (var i in renderers)
                foreach (var j in i.materials)
                    j.SetColor("_Color", color);
        }

        public void AddBullets(int num)
        {
            GameDefine.Haptic(HandRole.LeftHand, 100);
            for (int i = 0; i < num; i++)
            {
                Vector3 pos = new Vector3(-0.06f, Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f));
                photonView.RPC("AS_Player_ActiveBulletInShield",
                               RpcTarget.All,
                               pos
                               );
            }
        }

        public GameObject GetPlayerBody()
        {
            return body;
        }

        public GameObject GetPlayerShield()
        {
            if (weaponType == WeaponType.GunShield)
                return shield;
            else if (weaponType == WeaponType.ShieldV2)
                return shield2;
            return gun;
        }

        public void ChangeWeaponType(WeaponType wt)
        {
            if (photonView.IsMine)
            {
                photonView.RPC("AS_Player_ChangeWeapon",
                               RpcTarget.All,
                               wt);
            }
        }

        private void CB_OpenEquipment()
        {
            if (weaponType == WeaponType.GunShield)
            {
                gun.gameObject.SetActive(true);
                gun.GetComponent<GunController>().ResetGun();
                shield.gameObject.SetActive(true);
                shield2.gameObject.SetActive(false);
                glove.gameObject.SetActive(false);
            }
            else if (weaponType == WeaponType.GunGlove)
            {
                gun.gameObject.SetActive(true);
                gun.GetComponent<GunController>().ResetGun();
                shield.gameObject.SetActive(false);
                shield2.gameObject.SetActive(false);
                glove.gameObject.SetActive(true);
            }
            else if (weaponType == WeaponType.ShieldV2)
            {
                gun.gameObject.SetActive(false);
                shield.gameObject.SetActive(false);
                shield2.gameObject.SetActive(true);
                shield2.GetComponent<ShieldController>().ResetGun();
                glove.gameObject.SetActive(false);
            }
            Debug.Log("[PlayerController][CB_OpenEquipment]");
        }

        private void CB_CloseEquipment()
        {
            gun.gameObject.SetActive(false);
            shield.SetActive(false);
            shield2.SetActive(false);
            glove.SetActive(false);
            Debug.Log("[PlayerController][CB_CloseEquipment]");
        }

        /// <summary>
        /// Photon RPC Functions
        /// </summary>   
        [PunRPC]
        private void AS_Player_DamageEffect(Vector3 relative)
        {
            injuredRenderer.material.SetTexture("_MainTex", injuredTexDir);

            if (relative.x < 0 && relative.z < 0.5f) //Left
                injuredRenderer.transform.localEulerAngles = new Vector3(0, 0, 90);
            else if (relative.x > 0 && relative.z < 0.5f) //Right
                injuredRenderer.transform.localEulerAngles = new Vector3(0, 0, 270);
            else if (relative.z < 0.5f) //behind
                injuredRenderer.transform.localEulerAngles = new Vector3(0, 0, 180);
            else if (relative.y > 0.5f && relative.x > -0.5f && relative.x < 0.5f && relative.z > -0.5f && relative.z < 0.5f) //Up
                injuredRenderer.transform.localEulerAngles = new Vector3(0, 0, 0);
            else
                injuredRenderer.material.SetTexture("_MainTex", injuredTex);

            var s = DOTween.Sequence();
            s.Append(injuredRenderer.material.DOColor(new Color32(255, 0, 0, 255), 0.2f).SetEase(Ease.OutExpo));
            s.Append(injuredRenderer.material.DOColor(new Color32(255, 0, 0, 0), 1f).SetEase(Ease.OutExpo));
        }

        [PunRPC]
        private void AS_Player_Haptic(int mode = -1)
        {
            if (mode == 0)
                GameDefine.Haptic(HandRole.LeftHand, 100);
            else if (mode == 1)
                GameDefine.Haptic(HandRole.RightHand, 100);
            else if (mode == 2)
            {
                GameDefine.Haptic(HandRole.LeftHand, 100);
                GameDefine.Haptic(HandRole.RightHand, 100);
            }
        }

        [PunRPC]
        private void AS_Player_ActiveBulletInShield(Vector3 relative)
        {
            if (weaponType == WeaponType.GunShield)
                gun.GetComponent<GunController>().ActiveBulletInMagazine(relative);
            else if (weaponType == WeaponType.ShieldV2)
                shield2.GetComponent<ShieldController>().ActiveBulletInMagazine(relative);
            else if (weaponType == WeaponType.GunGlove)
                gun.GetComponent<GunController>().ActiveBulletInMagazine(relative);
        }

        [PunRPC]
        private void AS_Player_DeactiveBulletInShield(int id)
        {
            if (weaponType == WeaponType.GunShield)
                gun.GetComponent<GunController>().DeactiveBulletInMagazine(id);
            else if (weaponType == WeaponType.ShieldV2)
                shield2.GetComponent<ShieldController>().DeactiveBulletInMagazine(id);
            else if (weaponType == WeaponType.GunGlove)
                gun.GetComponent<GunController>().DeactiveBulletInMagazine(id);
        }

        [PunRPC]
        private void AS_Player_ChangeWeapon(WeaponType wt)
        {
            weaponType = wt;
        }
    }
}