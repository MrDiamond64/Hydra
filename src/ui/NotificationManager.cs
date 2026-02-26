using System;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui
{
    public class NotificationManager : MonoBehaviour
    {
        public Vector2 boxSize = new Vector2(325, 90);
        public List<Notification> notifications = new List<Notification>();

        public void Update()
        {
            int notificaions = Math.Min(GetMaxNotifications(), notifications.Count);

            for(byte i = 0; i < notificaions; i++)
            {
                Notification notification = notifications[i];
                notification.lifetime += Time.deltaTime;

                if(notification.HasExpired)
                {
                    notifications.RemoveAt(i);

                    // Since we removed an element from the notifications list, we have to decrement both the current notification index
                    // and the max notifications to avoid errors from accessing outside the list length
                    i--;
                    notificaions--;
                    continue;
                }
            }
        }

        public void OnGUI()
        {
            int notificaions = Math.Min(GetMaxNotifications(), notifications.Count);

            for(byte i = 0; i < notificaions; i++)
            {
                RenderNotification(i, notifications[i]);
            }
        }

        private void RenderNotification(byte position, Notification notification)
        {
            float boxX = Screen.width - boxSize.x;
            float boxY = Screen.height - (int)(boxSize.y * (position + 1));

            GUI.Box(new Rect(boxX, boxY, boxSize.x, boxSize.y), notification.title);

            GUI.Label(new Rect(boxX + 10, boxY + 17, boxSize.x - 15, boxSize.y - 20), notification.message);

            GUI.HorizontalSlider(new Rect(boxX, boxY + 70, boxSize.x, boxSize.y), notification.ttl - notification.lifetime, 0, notification.ttl);
        }

        public int GetMaxNotifications()
        {
            return (Screen.height / 2) / (int)boxSize.y;
        }

        // The time to live value for a notification should be five seconds if it is a success message, and ten seconds if it is a failure message
        public void Send(string title, string message, float ttl = 10)
        {
			Hydra.Log.LogMessage($"[Notification] [{title}] {message}");

			Notification notification = new Notification(title, message, ttl);
            notifications.Add(notification);
        }

        public void ClearNotifications()
        {
            notifications.Clear();
        }
    }
}