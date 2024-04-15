using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using DG.Tweening;
using HTC.UnityPlugin.Vive;

namespace AnchorSharing
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class BulletController : MonoBehaviour, IColorable
    {
        public enum State
        {
            active = 0,
            inactive = 1
        }

        [Header("Colors: Player_1->Colors[0], Player_2->Colors[1]")]
        [SerializeField] private List<Color32> colors = new List<Color32>();
        [SerializeField] public State state = State.active;

        public List<Color32> Colors
        {
            set { colors = value; }
            get { return colors; }
        }

        [Header("Components")]
        [SerializeField] private PhotonView photonView = null;
        [SerializeField] private Rigidbody rigidbody = null;
        [SerializeField] private MeshRenderer meshRenderer = null;
        [SerializeField] private List<GameObject> particlePrefabs;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource = null;
        [SerializeField] private AudioClip audioClipShootByGun = null;
        [SerializeField] private AudioClip audioClipShootByCannon = null;
        [SerializeField] private AudioClip audioClipHitWall = null;
        [SerializeField] private AudioClip audioClipHitRebounder = null;

        [Header("Values")]
        [SerializeField] private float speed = 3f;
        [SerializeField] private int point = 1;
        [SerializeField] private bool debugMode = true;
        [SerializeField] private TextMesh id = null;

        private Vector3 currentDir = Vector3.zero;
        private int bounceTimes = 0;
        private bool isBouncing = false;
        private bool ignoreCollisionWithCannon = false;

        private IEnumerator Start()
        {
            object[] instantiationData = photonView.InstantiationData;
            state = (State)instantiationData[0];
            ignoreCollisionWithCannon = (bool)instantiationData[1];
            speed = (float)instantiationData[2];

            DisplayName();

            if (state == State.active)
            {
                if (photonView.Owner.NickName == GameDefine.PLAYER_1_NAME)
                    SetColor(colors[0]);
                else if (photonView.Owner.NickName == GameDefine.PLAYER_2_NAME)
                    SetColor(colors[1]);

                AddForce();
                if (ignoreCollisionWithCannon)
                    PlaySound(audioClipShootByCannon);
                else
                    PlaySound(audioClipShootByGun);

                if (photonView.IsMine)
                {
                    yield return new WaitForSeconds(10);
                    PhotonManager.Instance.DestoryObject(gameObject);
                }
            }
            else if (state == State.inactive)
            {
                float floatingAmplitude = Random.Range(0.1f, 0.2f);
                float floatingDuration = Random.Range(1, 1.5f);
                gameObject.transform.DOMoveY(gameObject.transform.position.y + floatingAmplitude, floatingDuration).SetLoops(-1, LoopType.Yoyo);
                foreach (var i in gameObject.GetComponentsInChildren<Collider>())
                    i.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine)
                return;

            if (other.gameObject.name == GameDefine.GUN_NAME)
                return;

            if (other.gameObject.name == GameDefine.HEAD_NAME ||
                other.gameObject.name == GameDefine.BODY_NAME)
            {
                if (state == State.inactive)
                    return;

                PhotonView seltPlayer = GameManager.Instance.selfPlayerController.GetComponent<PhotonView>();
                PhotonView ptvTarget = other.gameObject.GetComponentInParent<PhotonView>();
                Vector3 relative = other.gameObject.transform.InverseTransformPoint(transform.position);
                if (ptvTarget.Owner.NickName != seltPlayer.Owner.NickName)
                {
                    //Add Score
                    GameManager.Instance.scoreBoardController.AddTeamScore(seltPlayer.Owner.NickName, point);
                    //Enemy get hurt
                    ptvTarget.RPC("AS_Player_Haptic",
                                  RpcTarget.Others,
                                  2
                                  );
                    ptvTarget.RPC("AS_Player_DamageEffect",
                                  RpcTarget.Others,
                                  relative
                                  );
                }

                PhotonManager.Instance.DestoryObject(gameObject);
                return;
            }

            if (other.gameObject.name == GameDefine.SHIELD_INNER_NAME)
            {
                PhotonView ptvTarget = other.gameObject.GetComponentInParent<PhotonView>();
                Vector3 relative = other.transform.parent.InverseTransformPoint(transform.position);

                if (ptvTarget.Owner.NickName != photonView.Owner.NickName)
                    ptvTarget.RPC("AS_Player_Haptic", 
                                  RpcTarget.Others, 
                                  0
                                  );
                else if (ptvTarget.Owner.NickName == photonView.Owner.NickName)
                    GameDefine.Haptic(HandRole.LeftHand, 100);

                ptvTarget.RPC("AS_Player_ActiveBulletInShield",
                              RpcTarget.All,
                              relative
                              );

                PhotonManager.Instance.DestoryObject(gameObject);
                return;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!photonView.IsMine)
                return;

            if (isBouncing)
                return;

            if (collision.gameObject.name == GameDefine.SHIELD_NAME ||
                collision.gameObject.tag == GameDefine.TAG_BULLET ||
                collision.gameObject.tag == GameDefine.TAG_WALL)
            {
                PlaySound(audioClipHitWall);

                Vector3 newDir = Vector3.Reflect(currentDir, collision.contacts[0].normal);
                photonView.RPC("AS_Bullet_Bouncing",
                               RpcTarget.All,
                               transform.position,
                               speed,
                               newDir,
                               0
                               );
                return;
            }

            if (collision.gameObject.name == GameDefine.GLOVE_NAME)
            {
                PlaySound(audioClipHitWall);

                Vector3 newDir = Vector3.Reflect(currentDir, collision.contacts[0].normal);
                photonView.RPC("AS_Bullet_Bouncing",
                               RpcTarget.All,
                               transform.position,
                               5f,
                               newDir,
                               0
                               );
                return;
            }

            if (collision.gameObject.tag == GameDefine.TAG_REBOUNDER)
            {
                PlaySound(audioClipHitRebounder);

                Vector3 newDir = Vector3.Reflect(currentDir, collision.contacts[0].normal);
                ReboundController reboundController = collision.transform.parent.gameObject.GetComponent<ReboundController>();

                if (reboundController.GetRebounderType() == ReboundController.Type.Acceleration)
                    speed = 5f;
                else if (reboundController.GetRebounderType() == ReboundController.Type.Deceleration)
                    speed = 1f;
                else if (reboundController.GetRebounderType() == ReboundController.Type.SpecifiedObject)
                    speed = 3f;

                if (GameManager.Instance.otherPlayerController != null)
                    newDir = GameManager.Instance.otherPlayerController.transform.GetChild(0).position - transform.position;

                //Re Bouncing
                photonView.RPC("AS_Bullet_Bouncing",
                               RpcTarget.All,
                               transform.position,
                               speed,
                               newDir,
                               1
                               );

                //When Rebounder Hited (Shake and play sound)
                float strength = 0.05f;
                if (speed == 5)
                    strength = 0.1f;
                else if (speed == 1f)
                    strength = 0.01f;
                else
                    strength = 0.05f;
                collision.transform.parent.gameObject.GetPhotonView().RPC("AS_Rebounder_Hit",
                                                                          RpcTarget.All,
                                                                          strength,
                                                                          newDir
                                                                          );

                //Change New Rebounder Type
                System.Array values = System.Enum.GetValues(typeof(ReboundController.Type));
                int randomNum = Random.Range(0, values.Length);
                collision.transform.parent.gameObject.GetPhotonView().RPC("AS_Rebounder_ChangeType",
                                                                          RpcTarget.All,
                                                                          (ReboundController.Type)randomNum,
                                                                          -1
                                                                          );
                return;
            }

            if (collision.gameObject.tag == GameDefine.TAG_CANNON)
            {
                if (!ignoreCollisionWithCannon)
                {
                    collision.gameObject.GetPhotonView().RPC("AS_Cannon_UpdateValue",
                                                             RpcTarget.All,
                                                             photonView.Owner.NickName
                                                             );

                    PhotonManager.Instance.DestoryObject(gameObject);
                }
                return;
            }
        }

        public void SetColor(Color32 color)
        {
            meshRenderer.material.color = color;
        }

        private void DisplayName()
        {
            if (debugMode)
                id.text = photonView.ViewID.ToString();
            else
                id.text = "";
        }

        private void PlaySound(AudioClip audioClip)
        {
            audioSource.Stop();
            audioSource.clip = audioClip;
            audioSource.Play();
        }

        private void AddForce()
        {
            //Add Force
            rigidbody.velocity = transform.forward * speed;
            currentDir = transform.forward;
        }

        private IEnumerator PreventRepeatedBouncing(int waitFrames)
        {
            while (waitFrames > 0)
            {
                yield return null;
                waitFrames--;
            }
            isBouncing = false;
        }

        /// <summary>
        /// Photon RPC Functions
        /// </summary>   
        [PunRPC]
        private void AS_Bullet_Bouncing(Vector3 pos, float speed, Vector3 dir, int particle)
        {
            if (isBouncing) 
                return;

            isBouncing = true;
            StartCoroutine(PreventRepeatedBouncing(5));

            Instantiate(particlePrefabs[particle], pos, Quaternion.identity);
            bounceTimes++;
            if (bounceTimes >= 3 && photonView.IsMine)
            {
                PhotonManager.Instance.DestoryObject(gameObject);
                return;
            }

            gameObject.transform.position = pos;

            currentDir = dir.normalized;
            this.speed = speed;
            rigidbody.velocity = currentDir * speed;
        }
    }
}