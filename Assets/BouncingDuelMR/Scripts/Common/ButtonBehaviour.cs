using HTC.UnityPlugin.Vive;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnchorSharing
{
    public class ButtonBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            GameDefine.Haptic(HandRole.RightHand, 10);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameDefine.Haptic(HandRole.RightHand, 10);
        }
    }
}