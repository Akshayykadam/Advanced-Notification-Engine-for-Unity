using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DevOrbit.AdvancedNotificationEngine.Runtime.Core;
using DevOrbit.AdvancedNotificationEngine.Runtime.Models;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.UI
{
    /// <summary>
    /// Controls the in-app notification overlay UI.
    /// </summary>
    public class NotificationOverlayController : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject Panel;
        public Text TitleText;
        public Text BodyText;
        public Image IconImage;

        [Header("Settings")]
        public float DisplayDuration = 3.0f;

        private void Start()
        {
            if (Panel != null) Panel.SetActive(false);
            NotificationManager.OnNotificationReceived += ShowOverlay;
        }

        private void OnDestroy()
        {
            NotificationManager.OnNotificationReceived -= ShowOverlay;
        }

        private void ShowOverlay(LocalNotificationRequest request)
        {
            if (Panel == null) return;

            // Set content
            if (TitleText != null) TitleText.text = request.Title;
            if (BodyText != null) BodyText.text = request.Body;
            
            // Show panel
            Panel.SetActive(true);
            
            // Auto-hide
            StopAllCoroutines();
            StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(DisplayDuration);
            Panel.SetActive(false);
        }
    }
}
