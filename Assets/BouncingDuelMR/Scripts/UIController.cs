using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnchorSharing
{
    public class UIController : MonoBehaviour
    {
        private static UIController instance;

        public static UIController Instance
        {
            get
            {
                if (instance == null)
                    instance = new UIController();
                return instance;
            }
        }
        public string RoomNumber { get; private set; } = "";

        [SerializeField] private GameObject UIGroup;
        [SerializeField] private List<GameObject> pages = new List<GameObject>();

        private bool weaponToggle;

        [Space]
        [Header("Page 0")]
        [SerializeField] private TMP_Dropdown p0_Dropdown;
        [SerializeField] private TMP_Text p0_MessageText;
        [SerializeField] private Button p0_ConnectToServerButton;

        [Space]
        [Header("Page 1")]
        [SerializeField] private TMP_Text p1_RoomNumInputField;
        [SerializeField] private Button p1_JoinRoomButton;
        [SerializeField] private Button p1_CreatRoomButton;
        [SerializeField] private Button p1_BackButton;

        [Space]
        [Header("Page 2")]
        [SerializeField] private TMP_Text p2_MessageText;
        [SerializeField] private TMP_Text p2_RoomNumberText;
        [SerializeField] private Image P2_ImgLoading;
        [SerializeField] private Button p2_LeaveRoomButton;

        [Space]
        [Header("Page 3")]
        [SerializeField] private TMP_Text p3_MessageText;
        [SerializeField] private Button p3_LeaveRoomButton;

        [Space]
        [Header("Page 4")]
        [SerializeField] private TMP_Text p4_MessageText;
        [SerializeField] private Slider p4_ProgressBar;

        [Space]
        [Header("Page 5")]
        [SerializeField] private TMP_Text p5_MessageText;
        [SerializeField] private Button p5_UpdateAnchorButton;
        [SerializeField] private Button p5_ConfirmAnchorButton;

        [Space]
        [Header("Page 6")]
        [SerializeField] private TMP_Text p6_MessageText;
        [SerializeField] private Button p6_ReimportAnchorButton;

        [Space]
        [Header("Page 7")]
        [SerializeField] private Toggle p7_LeftToggle;
        [SerializeField] private Toggle p7_RightToggle;
        [SerializeField] private Button p7_ConfirmButton;

        [Space]
        [Header("Page 8")]
        [SerializeField] private Button p8_BackButton;
        [SerializeField] private Button p8_NextButton;
        [SerializeField] private Button p8_CloseButton;
        [SerializeField] private List<GameObject> p8_TutorialPageList;
        private int tutorialCurrentPage = 0;

        [Space]
        [Header("Page 9")]
        [SerializeField] private Button p9_StartGameButton;
        [SerializeField] private Button p9_ReadyButton;
        [SerializeField] private Button p9_WeaponButton;
        [SerializeField] private Button p9_TutorialButton;
        [SerializeField] private TMP_Text p9_MessageText;
        private bool clientReady = false;

        [Space]
        [Header("Page 10")]
        [SerializeField] private Button p10_BackButton;

        [Space]
        [Header("Page 11")]
        [SerializeField] private TMP_Text p11_ScoreText;
        [SerializeField] private Button p11_ReplayButton;
        [SerializeField] private Button p11_BackToLobbyButton;

        public PageName CurrentPage { get; private set; } = PageName.None;

        [SerializeField] private GameObject vivePointer = null;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            //Register
            PhotonCallbacks.Instance.punEvent += PhotonCB;

            GameManager.Instance.OnGameStart.AddListener(delegate ()
            {
                CloseUI();
                vivePointer.SetActive(false);
            });
            GameManager.Instance.OnGameEnd.AddListener(delegate ()
            {
                ToPage(PageName.GameOverPage);
                OpenUI();
                vivePointer.SetActive(true);
            });

            AnchorAlignmentManager.Instance.OnFirstTimeLocateAnchorDone.AddListener(delegate ()
            {
                Debug.Log("[UIController][AnchorAlignmentManager.OnFirstTimeLocateAnchorDone]");
                p5_ConfirmAnchorButton.interactable = true;
            });

            p0_ConnectToServerButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p0_ConnectToServerButton]");
                p0_ConnectToServerButton.interactable = false;
                GameManager.Instance.StartConnecting();
                ShowLog(PageName.FrontPage, "Connecting...");
            });

            p1_CreatRoomButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p1_CreatRoomButton]");
                CreateRoomByNumber();
            });

            p1_JoinRoomButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p1_JoinRoomButton]");
                JoinRoomByNumber();
            });

            p1_BackButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p1_BackButton]");
                ToPage(PageName.FrontPage);
            });

            p2_LeaveRoomButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p2_LeaveRoomButton]");
                if (PhotonManager.Instance.InRoom())
                    PhotonManager.Instance.LeaveRoom();
                ToPage(PageName.FrontPage);
            });

            p3_LeaveRoomButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p3_LeaveRoomButton]");
                if (PhotonManager.Instance.InRoom())
                    PhotonManager.Instance.LeaveRoom();
                ToPage(PageName.FrontPage);
            });

            p5_ConfirmAnchorButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p5_ConfirmAnchorButton] Confirm Final Anchor Point");
                ShowLog(PageName.ClientConfirmAnchorPage, "Confirm anchor location");
                AnchorAlignmentManager.Instance.ConfirmFinalAnchorPoint();
                PhotonManager.Instance.SetRoomProperty(GameDefine.ANCHOR_ALIGNMENT_SUCCESS, true);
            });

            //p5_UpdateAnchorButton.onClick.AddListener(delegate ()
            //{
            //    Debug.Log("[UIController][p5_UpdateAnchorButton] Realign Anchor");
            //    UIController.Instance.ShowLog(PageName.ClientConfirmAnchorPage, $"重新匯入錨點...");
            //    StartCoroutine(AnchorAlignmentManager.Instance.RealignAnchor());
            //});

            p6_ReimportAnchorButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p6_ReimportAnchorButton]");
                p6_ReimportAnchorButton.interactable = false;
                GameManager.Instance.scoreBoardController.GetComponent<PhotonView>().RPC("AS_RecreateAnchor", RpcTarget.All);
            });

            p7_LeftToggle.onValueChanged.AddListener(delegate (bool isOn) 
            {
                if (isOn == p7_RightToggle.isOn) 
                {
                    p7_RightToggle.isOn = !isOn;
                }
                Debug.Log($"[UIController][p7_LeftToggle] Left: {p7_LeftToggle.isOn} Right: {p7_RightToggle.isOn}");
            });

            p7_RightToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                if (isOn == p7_LeftToggle.isOn)
                {
                    p7_LeftToggle.isOn = !isOn;
                }
                Debug.Log($"[UIController][p7_RightToggle] Left: {p7_LeftToggle.isOn} Right: {p7_RightToggle.isOn}");
            });

            p7_ConfirmButton.onClick.AddListener(delegate ()
            {
                if (p7_LeftToggle.isOn == p7_RightToggle.isOn)
                {
                    Debug.LogError($"[UIController][p7_ConfirmButton] two toggles with same value({p7_LeftToggle.isOn}), use old weapon as default.");
                    GameManager.Instance.selfPlayerController.ChangeWeaponType(PlayerController.WeaponType.GunShield);
                }
                else if (p7_LeftToggle.isOn)
                {
                    Debug.Log("[UIController][p7_ConfirmButton] use Left (Gun & Boxing glove)");
                    GameManager.Instance.selfPlayerController.ChangeWeaponType(PlayerController.WeaponType.GunGlove);
                }
                else
                {
                    Debug.Log("[UIController][p7_ConfirmButton] use Right (Slingshot Shield)");
                    GameManager.Instance.selfPlayerController.ChangeWeaponType(PlayerController.WeaponType.ShieldV2);
                }
                ToPage(PageName.MenuPage);
            });

            p8_NextButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p8_NextButton]");
                if (tutorialCurrentPage < p8_TutorialPageList.Count - 1) 
                {
                    p8_TutorialPageList[tutorialCurrentPage].SetActive(false);
                    tutorialCurrentPage++;
                    p8_TutorialPageList[tutorialCurrentPage].SetActive(true);
                }
                p8_NextButton.gameObject.SetActive(tutorialCurrentPage != p8_TutorialPageList.Count - 1);
                p8_BackButton.gameObject.SetActive(tutorialCurrentPage != 0);
            });

            p8_BackButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p8_BackButton]");
                if (tutorialCurrentPage > 0)
                {
                    p8_TutorialPageList[tutorialCurrentPage].SetActive(false);
                    tutorialCurrentPage--;
                    p8_TutorialPageList[tutorialCurrentPage].SetActive(true);
                }
                p8_NextButton.gameObject.SetActive(tutorialCurrentPage != p8_TutorialPageList.Count - 1);
                p8_BackButton.gameObject.SetActive(tutorialCurrentPage != 0);
            });

            p8_CloseButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p8_CloseButton]");
                ToPage(PageName.MenuPage);
            });

            p9_StartGameButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p9_StartGameButton]");
                p9_StartGameButton.interactable = false;
                GameManager.Instance.scoreBoardController.GetComponent<PhotonView>().RPC("AS_GameRestart", RpcTarget.All);
                clientReady = false;
            });

            p9_ReadyButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p9_ReadyButton]");
                p9_ReadyButton.interactable = false;
                p9_TutorialButton.interactable = false;
                p9_WeaponButton.interactable = false;
                GameManager.Instance.scoreBoardController.GetComponent<PhotonView>().RPC("AS_ClientReady", RpcTarget.All);
            });

            p9_WeaponButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p9_WeaponButton]");
                ToPage(PageName.SelectWeaponPage);
            });

            p9_TutorialButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p9_TutorialButton]");
                ToPage(PageName.TutorialPage);
            });

            p11_ReplayButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p11_ReplayButton]");
                p11_ReplayButton.interactable = false;
                ToPage(PageName.MenuPage);
            });

            p11_BackToLobbyButton.onClick.AddListener(delegate ()
            {
                Debug.Log("[UIController][p11_BackToLobbyButton]");
                SocketManager.Instance.KillSocket();
                PhotonManager.Instance.LoadLevel(SceneManager.GetActiveScene().name);
            });

            ToPage(PageName.FrontPage);
        }

        private void Update()
        {
            if (P2_ImgLoading.gameObject.activeSelf)
                P2_ImgLoading.transform.Rotate(Vector3.forward, 100 * Time.deltaTime);
        }

        private void PhotonCB(object sender, PhotonCallbacks.PhotonCallbackEvent args)
        {
            switch (args.state)
            {
                case State.sOnConnected:
                    if(CurrentPage == PageName.FrontPage) ToPage(PageName.JoinOrCreatRoomPage);
                    break;
                case State.sOnConnectedToMaster:
                    break;
                case State.sOnDisconnected:
                    if (CurrentPage == PageName.FrontPage)
                    {
                        p0_ConnectToServerButton.interactable = true;
                        ShowLog(PageName.FrontPage, "<color=red>Connection failed. Please check your network.</color>");
                    }
                    else
                        PhotonManager.Instance.LoadLevel(SceneManager.GetActiveScene().name);
                    break;
                case State.sOnJoinedLobby:
                    break;
                case State.sOnCreatedRoom:
                    break;
                case State.sOnJoinedRoom:
                    break;
                case State.sOnLeftRoom:
                    ToPage(PageName.FrontPage);
                    break;
                case State.sOnCreateRoomFailed:
                    StartCoroutine(OnJoinOrCreateRoomFailed((string)args.data));
                    break;
                case State.sOnJoinRoomFailed:
                    StartCoroutine(OnJoinOrCreateRoomFailed((string)args.data));
                    break;
                case State.sOnPlayerEnteredRoom:
                    break;
                case State.sOnPlayerLeftRoom:
                    ToPage(PageName.FrontPage);
                    break;
                case State.sOnRoomListUpdate:
                    break;
                case State.sOnRoomPropertiesUpdate:
                    break;
            }
        }

        public void OpenUI()
        {
            UIGroup.SetActive(true);
        }

        public void CloseUI()
        {
            UIGroup.SetActive(false);
        }

        private IEnumerator OnJoinOrCreateRoomFailed(string message) 
        {
            Debug.Log($"[UIController][OnJoinOrCreateRoomFailed] Current Page: {CurrentPage}, Message: {message}");
            if (CurrentPage != PageName.LoadingForRoomPage) 
                yield break;

            ShowLog(PageName.LoadingForRoomPage, message);

            yield return new WaitForSeconds(3);

            if (PhotonNetwork.IsConnected)
            {
                if (!PhotonManager.Instance.InLobby())
                {
                    PhotonNetwork.JoinLobby();
                    yield return new WaitUntil(() => PhotonManager.Instance.InLobby());
                }
                ToPage(PageName.JoinOrCreatRoomPage);
            }
            else
            {
                ToPage(PageName.FrontPage);
            }
        }

        public void ToPage(PageName page)
        {
            Debug.Log($"[UIController][ToPage] Current Page: {CurrentPage}, To Page: {page}");

            if (CurrentPage == page || page == PageName.None) return;

            if (!UIGroup.activeSelf) OpenUI();

            if (CurrentPage >= 0) pages[(int)CurrentPage].SetActive(false);
            pages[(int)page].SetActive(true);
            PageName prevPage = CurrentPage;
            CurrentPage = page;
            ResetPage(prevPage);

            switch (page)
            {
                case PageName.FrontPage:
                    p0_ConnectToServerButton.interactable = true;
                    p0_Dropdown.value = (int)PhotonManager.Instance.region;
                    break;
                case PageName.JoinOrCreatRoomPage:
                    RoomNumber = "";
                    break;
                case PageName.LoadingForRoomPage:
                    p2_RoomNumberText.text = "Room Number : " + RoomNumber;
                    break;
                case PageName.WaitingCreatAnchorPage:
                    break;
                case PageName.SendingAnchorPage:
                    break;
                case PageName.ClientConfirmAnchorPage:
                    break;
                case PageName.MasterConfirmAnchorPage:
                    break;
                case PageName.SelectWeaponPage:
                    break;
                case PageName.TutorialPage:
                    tutorialCurrentPage = 0;
                    foreach (GameObject tp in p8_TutorialPageList) 
                    {
                    tp.SetActive(false);
                    }
                    p8_TutorialPageList[tutorialCurrentPage].SetActive(true);
                    p8_BackButton.gameObject.SetActive(false);
                    p8_NextButton.gameObject.SetActive(true);
                    break;
                case PageName.MenuPage:
                    if (PhotonManager.Instance.IsMasterClient())
                    {
                        p9_StartGameButton.gameObject.SetActive(true);
                        p9_StartGameButton.interactable = clientReady;
                        p9_ReadyButton.gameObject.SetActive(false);
                        if (!clientReady)
                            ShowLog(PageName.MenuPage, "After the client presses the [Ready to play] button,\n" +
                            "the [Start Game] button will become interactive.");
                        else
                            ShowLog(PageName.MenuPage, "The client is ready! You can start the game at any time.");
                    }
                    else 
                    {
                        p9_StartGameButton.gameObject.SetActive(false);
                        p9_ReadyButton.gameObject.SetActive(true);
                        p9_ReadyButton.interactable = true;
                    }
                    p9_TutorialButton.interactable = true;
                    p9_WeaponButton.interactable = true;
                    break;
                case PageName.SetupRoomPage:
                    break;
                case PageName.GameOverPage:
                    p11_ReplayButton.interactable = true;
                    if (PhotonManager.Instance.IsMasterClient())
                    {
                        p11_ScoreText.text = GameManager.Instance.scoreBoardController.team1Score.ToString();
                    }
                    else
                    {
                        p11_ScoreText.text = GameManager.Instance.scoreBoardController.team2Score.ToString();
                    }
                    break;
                default:
                    break;
            }
        }

        private void ResetPage(PageName page)
        {
            Debug.Log($"[UIController][ResetPage] Current Page: {CurrentPage}, Reset Page: {page}");

            if (CurrentPage == page || page == PageName.None) return;

            switch (page)
            {
                case PageName.FrontPage:
                    p0_MessageText.text = "";
                    break;
                case PageName.JoinOrCreatRoomPage:
                    p1_RoomNumInputField.text = "ENTER 4-digit ROOM NUMBER";
                    break;
                case PageName.LoadingForRoomPage:
                    p2_MessageText.text = "";
                    break;
                case PageName.WaitingCreatAnchorPage:
                    p3_MessageText.text = "";
                    break;
                case PageName.SendingAnchorPage:
                    p4_MessageText.text = "";
                    p4_ProgressBar.gameObject.SetActive(false);
                    break;
                case PageName.ClientConfirmAnchorPage:
                    p5_MessageText.text = "";
                    p5_ConfirmAnchorButton.interactable = false;
                    p5_UpdateAnchorButton.interactable = false;
                    break;
                case PageName.MasterConfirmAnchorPage:
                    p6_MessageText.text = "";
                    p6_ReimportAnchorButton.interactable = true;
                    break;
                case PageName.SelectWeaponPage:
                    break;
                case PageName.TutorialPage:
                    break;
                case PageName.MenuPage:
                    p9_StartGameButton.gameObject.SetActive(false);
                    p9_ReadyButton.gameObject.SetActive(false);
                    p9_MessageText.text = "";
                    break;
                case PageName.SetupRoomPage:
                    break;
                case PageName.GameOverPage:
                    p11_ScoreText.text = "00";
                    break;
                default:
                    break;
            }
        }

        public void ShowLog(PageName targetPage, string msg)
        {
            Debug.Log($"[UIController][ShowLog] TargetPage: {targetPage} (current: {CurrentPage}), Message: {msg}");
            if (targetPage != CurrentPage || targetPage == PageName.None) return;

            switch (CurrentPage)
            {
                case PageName.FrontPage:
                    p0_MessageText.text = msg;
                    break;
                case PageName.LoadingForRoomPage:
                    p2_MessageText.text = msg;
                    break;
                case PageName.WaitingCreatAnchorPage:
                    p3_MessageText.text = msg;
                    break;
                case PageName.SendingAnchorPage:
                    p4_MessageText.text = msg;
                    break;
                case PageName.ClientConfirmAnchorPage:
                    p5_MessageText.text = msg;
                    break;
                case PageName.MasterConfirmAnchorPage:
                    p6_MessageText.text = msg;
                    break; 
                case PageName.MenuPage:
                    p9_MessageText.text = msg;
                    break;
            }
        }

        public void UpdateProgressBar(float progress)
        {
            if (CurrentPage != PageName.SendingAnchorPage) return;
            p4_ProgressBar.gameObject.SetActive(true);
            p4_ProgressBar.value = progress;
        }

        public void NumericKey(int num)
        {
            if (CurrentPage != PageName.JoinOrCreatRoomPage || num < 0 || num > 9 || RoomNumber.Length >= 4)
                return;

            RoomNumber += num.ToString();

            p1_RoomNumInputField.text = RoomNumber;

            Debug.Log($"[UIController][NumericKey] add {num}, current RoomNumber: {RoomNumber}");
        }

        public void ClearRoomNumber()
        {
            if (CurrentPage != PageName.JoinOrCreatRoomPage)
                return;

            RoomNumber = "";

            p1_RoomNumInputField.text = "ENTER 4-digit ROOM NUMBER";
            Debug.Log($"[UIController][ClearRoomNumber]");
        }

        public void DeleteRoomNumber()
        {
            if (CurrentPage != PageName.JoinOrCreatRoomPage || RoomNumber.Length == 0)
                return;

            RoomNumber = RoomNumber.Remove(RoomNumber.Length - 1);

            if(RoomNumber.Length == 0)
                p1_RoomNumInputField.text = "ENTER 4-digit ROOM NUMBER";
            else
                p1_RoomNumInputField.text = RoomNumber;

            Debug.Log($"[UIController][DeleteRoomNumber] current RoomNumber: {RoomNumber}");
        }

        public void CreateRoomByNumber() 
        {
            if (!PhotonManager.Instance.InRoom() && RoomNumber.Length == 4) 
            {
                ToPage(PageName.LoadingForRoomPage);
                PhotonManager.Instance.CreateRoom(RoomNumber);
                Debug.Log($"[UIController][CreateRoomByNumber] create room [{RoomNumber}]");
            }
            else
            {
                RoomNumber = "";
                p1_RoomNumInputField.text = "Invalid Room Number";
                Debug.LogError($"[UIController][CreateRoomByNumber] Invalid Room Number");
            }
        }

        public void JoinRoomByNumber() 
        {
            if (!PhotonManager.Instance.InRoom() && RoomNumber.Length == 4)
            {
                ToPage(PageName.LoadingForRoomPage);
                PhotonManager.Instance.JoinRoom(RoomNumber);
                Debug.Log($"[UIController][JoinRoomByNumber] join room [{RoomNumber}]");
            }
            else
            {
                Debug.LogError($"[UIController][JoinRoomByNumber] Invalid Room Number");
                RoomNumber = "";
                p1_RoomNumInputField.text = "Invalid Room Number";
            }
        }

        public void OnRegionChanged(int value) 
        {
            PhotonManager.Instance.region = (PhotonManager.Region)value;
        }


        public void ClientReady()
        {
            if (PhotonManager.Instance.IsMasterClient() && !clientReady) 
            {
                StartCoroutine(WaitForMasterInMenuPage());
            }
            else
            {
                ShowLog(PageName.MenuPage, "You are ready to play! Wait for host to start the game.");
            }
        }

        private IEnumerator WaitForMasterInMenuPage() 
        {
            clientReady = true;
            yield return new WaitUntil(() => CurrentPage == PageName.MenuPage);
            yield return new WaitForEndOfFrame();
            p9_StartGameButton.interactable = clientReady;
            ShowLog(PageName.MenuPage, "The client is ready! You can start the game at any time.");
        }
    }

    public enum PageName
    {
        None = -1,
        FrontPage = 0,
        JoinOrCreatRoomPage = 1,
        LoadingForRoomPage = 2,
        WaitingCreatAnchorPage = 3,
        SendingAnchorPage = 4,
        ClientConfirmAnchorPage = 5,
        MasterConfirmAnchorPage = 6,
        SelectWeaponPage = 7,
        TutorialPage = 8,
        MenuPage = 9,
        SetupRoomPage = 10,
        GameOverPage = 11
    }
}