using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnchorSharing
{
    public class ScoreBoardController : MonoBehaviour, IPunObservable
    {
        public List<TMP_Text> TimeText = new List<TMP_Text>();
        public List<TMP_Text> ScoreText_Team1 = new List<TMP_Text>();
        public List<TMP_Text> ScoreText_Team2 = new List<TMP_Text>();
        public List<TMP_Text> Winner_Text = new List<TMP_Text>();
        public GameObject vfx = null;
        public List<GameObject> vfxFirework = new List<GameObject>();

        public bool IsGameRunning { get; private set; } = false;
        public int team1Score { get; private set; } = 0;
        public int team2Score { get; private set; } = 0;

        [SerializeField] private PhotonView photonView;
        [SerializeField] private float currentGameTime = 0;
        [SerializeField] private Animator scorePlusAni_B;
        [SerializeField] private Animator scorePlusAni_R;


        private void Start()
        {
            gameObject.transform.localScale = Vector3.zero;
            var s = DOTween.Sequence();
            s.Append(gameObject.transform.DOScale(Vector3.one, 1.25f).SetEase(Ease.OutElastic));
            s.Append(gameObject.transform.DOMoveY(gameObject.transform.position.y + 0.1f, 1).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear));

            scorePlusAni_R.gameObject.SetActive(false); 
            scorePlusAni_B.gameObject.SetActive(false);
        }

        //Only MasterClient will run this function.
        public void CallStartGameRPC()
        {
            if (PhotonManager.Instance.IsMasterClient())
            {
                Debug.Log("[ScoreBoardController][CallStartGameRPC]");
                photonView.RPC("AS_GameStart",
                               RpcTarget.All);
            }
        }

        //When anyone bullet hit players.
        public void AddTeamScore(string teamPlayerName, int addScore)
        {
            if (!IsGameRunning)
                return;
            photonView.RPC("AS_ScoreBoard_AddScore",
                           RpcTarget.All,
                           teamPlayerName,
                           addScore
                           );
            Debug.Log($"[ScoreBoardController][AddTeamScore]");
        }

        private IEnumerator Counting()
        {
            Debug.Log("[ScoreBoardController][Counting]");
            if (PhotonManager.Instance.IsMasterClient())
            {
                currentGameTime = 0;
                while (currentGameTime <= GameDefine.TIME_LIMIT)
                {
                    currentGameTime += Time.deltaTime;
                    UpdateTimerUI();
                    yield return null;
                }
                currentGameTime = GameDefine.TIME_LIMIT;
                photonView.RPC("AS_GameEnd",
                               RpcTarget.All);
            }
            else
            {
                while (currentGameTime <= GameDefine.TIME_LIMIT)
                {
                    UpdateTimerUI();
                    yield return null;
                }
            }
        }

        public void ResetScoreBoard()
        {
            currentGameTime = 0;
            team1Score = 0;
            team2Score = 0;
            foreach (TMP_Text text in TimeText)
            {
                text.text = "00:00";
            }
            foreach (TMP_Text text in ScoreText_Team1)
            {
                text.text = "00";
            }
            foreach (TMP_Text text in ScoreText_Team2)
            {
                text.text = "00";
            }
        }

        private void UpdateScoreUI()
        {
            string t1_score;
            if (team1Score == 0) t1_score = "00";
            else if (team1Score > 9) t1_score = team1Score.ToString();
            else t1_score = "0" + team1Score.ToString();

            foreach (TMP_Text text in ScoreText_Team1)
                text.text = t1_score;

            string t2_score;
            if (team2Score == 0) t2_score = "00";
            else if (team2Score > 9) t2_score = team2Score.ToString();
            else t2_score = "0" + team2Score.ToString();

            foreach (TMP_Text text in ScoreText_Team2)
                text.text = t2_score;
        }

        private void UpdateTimerUI()
        {
            float t = currentGameTime;
            t = Mathf.Clamp(t, 0, GameDefine.TIME_LIMIT);

            int m = (int)Mathf.Floor(t / 60);
            string minute;

            if (m == 0) minute = "00";
            else if (m > 9) minute = m.ToString();
            else minute = "0" + m.ToString();

            int s = (int)Mathf.Floor(t % 60);
            string second;

            if (s == 0) second = "00";
            else if (s > 9) second = s.ToString();
            else second = "0" + s.ToString();

            string timeText = minute + ":" + second;

            foreach (TMP_Text text in TimeText)
                text.text = timeText;
        }

        private IEnumerator Fireworks()
        {
            int times = 0;
            while (true)
            {
                times++;
                foreach (var i in vfxFirework)
                {
                    Vector3 position = Random.insideUnitSphere * 2.5f;
                    Vector3 newPosition = new Vector3(position.x, -2.5f, position.z);

                    i.transform.localPosition = newPosition;
                    i.SetActive(true);

                    yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
                }

                yield return new WaitForSeconds(Random.Range(3f, 3.5f));

                foreach (var i in vfxFirework)
                    i.SetActive(false);


                if (times > 2)
                    yield break;
            }
        }

        [PunRPC]
        private void AS_GameStart()
        {
            if (PhotonManager.Instance.IsMasterClient())
                IsGameRunning = true;

            //Players event lisner possibly doesn't work.
            AnchorAlignmentManager.Instance.ResetAnchorData();
            GameManager.Instance.OnGameStart.Invoke();
            StartCoroutine(Counting());
            Debug.Log("[ScoreBoardController][AS_GameStart]");

            foreach (var i in TimeText)
                i.transform.parent.gameObject.SetActive(true);
            foreach (var i in Winner_Text)
                i.transform.parent.gameObject.SetActive(false);
            vfx.SetActive(false);
        }

        [PunRPC]
        private void AS_GameEnd()
        {
            if (PhotonManager.Instance.IsMasterClient())
                IsGameRunning = false;
            currentGameTime = GameDefine.TIME_LIMIT;
            UpdateTimerUI();
            //ResetScoreBoard();
            GameManager.Instance.OnGameEnd.Invoke();
            Debug.Log("[ScoreBoardController][AS_GameEnd]");

            foreach (var i in TimeText)
                i.transform.parent.gameObject.SetActive(false);
            foreach (var i in Winner_Text)
            {
                i.transform.parent.gameObject.SetActive(true);
                if (team1Score > team2Score)
                    i.text = "PLAYER 1";
                else if (team1Score < team2Score)
                    i.text = "PLAYER 2";
                else
                    i.text = "NOBODY";
            }
            if (photonView.IsMine && team1Score > team2Score)
            {
                vfx.SetActive(true);
                StartCoroutine(Fireworks());
            }
            else if (!photonView.IsMine && team1Score < team2Score)
            {
                vfx.SetActive(true);
                StartCoroutine(Fireworks());
            }
            else
            {
                vfx.SetActive(false);
            }
        }

        [PunRPC]
        public void AS_GameRestart()
        {
            if (UIController.Instance.CurrentPage != PageName.MenuPage) UIController.Instance.ToPage(PageName.MenuPage);
            GameManager.Instance.RestartGame();
            Debug.Log("[ScoreBoardController][AS_GameRestart]");

            foreach (var i in TimeText)
                i.transform.parent.gameObject.SetActive(true);
            foreach (var i in Winner_Text)
                i.transform.parent.gameObject.SetActive(false);

            vfx.SetActive(false);
            foreach (var i in vfxFirework)
                i.SetActive(false);
            
            StopAllCoroutines();
        }

        [PunRPC]
        private void AS_ClientReady() 
        {
            UIController.Instance.ClientReady();
        }

        [PunRPC]
        public void AS_RecreateAnchor()
        {
            AnchorAlignmentManager.Instance.ResetAnchorData();
            GameManager.Instance.RecreateAnchor();
            Debug.Log("[ScoreBoardController][AS_RecreateAnchor]");
        }

        Sequence scorePlusMove_B;
        Sequence scorePlusMove_R;

        [PunRPC]
        private void AS_ScoreBoard_AddScore(string teamPlayerName, int addScore)
        {
            scorePlusAni_R.gameObject.SetActive(true);
            scorePlusAni_B.gameObject.SetActive(true);

            int indexR = scorePlusAni_R.transform.GetSiblingIndex();
            int indexB = scorePlusAni_B.transform.GetSiblingIndex();
            Vector3 playerPos = GameManager.Instance.selfPlayerController.GetPlayerBody().transform.position;

            if (teamPlayerName == GameDefine.PLAYER_1_NAME)
            {
                team1Score += addScore;
                team1Score = Mathf.Clamp(team1Score, 0, 99);

                if (indexB > indexR) scorePlusAni_R.transform.SetSiblingIndex(indexB);

                if (scorePlusMove_R != null && scorePlusMove_R.IsPlaying()) scorePlusMove_R.Kill();

                scorePlusAni_R.transform.localPosition = new Vector3(0, -1, 0);
                scorePlusMove_R.Append(scorePlusAni_R.transform.DOLocalMoveY(-0.8f, 0.75f));
                scorePlusAni_R.transform.LookAt(new Vector3(playerPos.x, scorePlusAni_R.transform.position.y, playerPos.z));
                scorePlusAni_R.Play("ScorePlus", 0, 0f);
            }
            else if (teamPlayerName == GameDefine.PLAYER_2_NAME)
            {
                team2Score += addScore;
                team2Score = Mathf.Clamp(team2Score, 0, 99);

                if (indexR > indexB) scorePlusAni_B.transform.SetSiblingIndex(indexR);

                if (scorePlusMove_B != null && scorePlusMove_B.IsPlaying()) scorePlusMove_B.Kill();

                scorePlusAni_B.transform.localPosition = new Vector3(0, -1, 0);
                scorePlusMove_B.Append(scorePlusAni_B.transform.DOLocalMoveY(-0.8f, 0.75f));
                scorePlusAni_B.transform.LookAt(new Vector3(playerPos.x, scorePlusAni_B.transform.position.y, playerPos.z));
                scorePlusAni_B.Play("ScorePlus", 0, 0f);
            }
            else
                Debug.LogError("[ScoreBoardController][AS_ScoreBoard_AddScore] Wrong player name");

            UpdateScoreUI();
            Debug.Log($"[ScoreBoardController][AS_ScoreBoard_AddScore] Team1 Score: {team1Score} Team2 Score: {team2Score}");
        }

        #region IPunObservable implementation
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(currentGameTime);
                stream.SendNext(IsGameRunning);
            }
            else
            {
                // Network player, receive data
                currentGameTime = (float)stream.ReceiveNext();
                IsGameRunning = (bool)stream.ReceiveNext();
            }
        }
        #endregion
    }
}