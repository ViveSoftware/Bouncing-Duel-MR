using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wave.Native;
using HTC.UnityPlugin.Vive;

namespace AnchorSharing
{
    public class GameDefine
    {
        /// <summary>
        /// Players Name
        /// </summary>
        public const string PLAYER_1_NAME = "Player1";
        public const string PLAYER_2_NAME = "Player2";

        /// <summary>
        /// PhotonNetwork Instantiate Names (Must Same As The "/Resources" Folder Prefabs Name)
        /// </summary>
        public const string PLAYER = "Player";
        public const string BULLET = "Bullet";
        public const string REBOUNDER = "Rebounder";
        public const string GASHAPON = "Gashapon";
        public const string CAPSULE = "Capsule";
        public const string CANNON = "Cannon";
        public const string SCOREBOARD = "ScoreBoard";

        /// <summary>
        /// Player Collision Area Name (Must Same As The "/Resources/Player" Prefab's Bodys Name)
        /// </summary>
        public const string HEAD_NAME = "Head";
        public const string HAND_L_NAME = "HandL";
        public const string HAND_R_NAME = "HandR";
        public const string BODY_NAME = "Body";

        /// <summary>
        /// Weapons
        /// </summary>
        public const string GUN_NAME = "Gun";
        public const string SHIELD_NAME = "shield";
        public const string SHIELD_INNER_NAME = "shieldInner";
        public const string GLOVE_NAME = "Glove";

        /// <summary>
        /// Tags
        /// </summary>
        public const string TAG_BULLET = "tag_bullet";
        public const string TAG_BULLET_STATIC = "tag_bullet_static";
        public const string TAG_WALL = "tag_wall";
        public const string TAG_REBOUNDER = "tag_rebounder";
        public const string TAG_CANNON = "tag_cannon";

        /// <summary>
        /// Layers
        /// </summary>
        public const string LAYER_CAMERA_IGNORE = "Layer_CameraIgnore";
        public const string LAYER_BULLET = "Layer_Bullet";
        public const string LAYER_BULLET_IGNORE = "Layer_BulletIgnore";

        /// <summary>
        /// Anchor Alignment
        /// </summary>
        public const string ANCHOR_ALIGNMENT_SUCCESS = "anchor_alignment_success";

        /// <summary>
        /// Socket
        /// </summary>
        public const string SOCKET_SERVER_IP = "socket_server_ip";
        public const string SOCKET_ANCHOR_SIZE = "socket_anchor_size";
        public const string SOCKET_ANCHOR_READY_TO_SYNC = "socket_anchor_ready_to_sync";

        /// <summary>
        /// Game Props
        /// </summary>
        public const int PLAYERS_COUNT = 2;
        public const int REBOUNDER_COUNT = 6;
        public const int GASHAPON_COUNT = 2; 
        public const int CANNON_COUNT = 1;

        public const int TIME_LIMIT = 80;

        public static async void Haptic(HandRole handRole, ushort strength)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            ViveInput.TriggerHapticPulse(handRole, strength);
            await Task.Delay(strength);
            ViveInput.TriggerHapticPulse(handRole, 0);
#endif
            await Task.Yield();
        }

        public static void EnablePassthrough(bool sw, WVR_PassthroughImageQuality mode)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        //Open Passthrough
        Interop.WVR_ShowPassthroughUnderlay(sw);
        Interop.WVR_SetPassthroughOverlayAlpha(0.5f);
        Interop.WVR_SetPassthroughImageQuality(mode);
        Interop.WVR_SetPassthroughImageFocus(WVR_PassthroughImageFocus.Scale);
        if (sw)
        {
            Camera.main.backgroundColor = new Color32(0, 0, 0, 0);
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }
        else if (!sw)
        {
            Camera.main.backgroundColor = new Color32(49, 77, 121, 179);
            Camera.main.clearFlags = CameraClearFlags.Skybox;
        }
#endif
        }
    }

    public class ASConfig 
    {
        public float boundarySizeParm = 0;
        public string photonRegion = "";
        public string photonAppID = "";
    }

    public class ASIO
    {

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        public static string asFileLocalPath = Path.Combine(Application.streamingAssetsPath, "asConfig.json");
#elif UNITY_ANDROID
        public static string asFileLocalPath = Path.Combine(Application.persistentDataPath, "asConfig.json");
#endif

        public static async Task<ASConfig> LoadLocalJsonFileAsync(string filePath)
        {
            try
            {
                string fileData = await File.ReadAllTextAsync(filePath, Encoding.UTF8);

                if (!string.IsNullOrEmpty(fileData))
                {
                    ASConfig data = await Task.Run(() => JsonConvert.DeserializeObject<ASConfig>(fileData));
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error loading and deserializing JSON file: {e.Message}");
                return null;
            }
        }
    }
}