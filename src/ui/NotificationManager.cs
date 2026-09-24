using System;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui
{
	internal class NotificationManager : MonoBehaviour
	{
		// The screen corner notifications are anchored to. Order matches the UI slider (0-3).
		public enum Corner
		{
			BottomRight,
			BottomLeft,
			TopRight,
			TopLeft
		}

		private readonly List<Notification> notifications = new List<Notification>();
		public bool disableNotifications = false;

		// The corner notifications stack from, and a user-facing cap on how many are shown at once.
		public Corner corner = Corner.BottomRight;
		public int maxNotifications = 5;

		public static Vector2 BoxSize
		{
			get { return new Vector2(325, 90) * MainUI.scale; }
		}

		public static Vector2 BoxHeaderSize
		{
			get { return new Vector2(BoxSize.x, 17 * MainUI.scale); }
		}

		public static Vector2 BoxContentPadding
		{
			get { return new Vector2(10, 0) * MainUI.scale; }
		}

		public static Vector2 BoxContentSize
		{
			get { return new Vector2(BoxSize.x - BoxContentPadding.x, BoxSize.y - BoxHeaderSize.y - BoxSliderSize.y); }
		}

		public static Vector2 BoxSliderSize
		{
			get { return new Vector2(BoxSize.x, 20 * MainUI.scale); }
		}

		public void Update()
		{
			// Age every notification, not just the visible ones, so notifications hidden behind the display
			// cap still expire instead of piling up forever.
			for(int i = 0; i < notifications.Count; i++)
			{
				Notification notification = notifications[i];
				notification.lifetime += Time.deltaTime;

				if(notification.HasExpired)
				{
					notifications.RemoveAt(i);
					i--;
				}
			}
		}

		public void OnGUI()
		{
			if(disableNotifications) return;

			int notificationCount = Math.Min(GetEffectiveMax(), notifications.Count);

			for(int i = 0; i < notificationCount; i++)
			{
				RenderNotification(i, notifications[i]);
			}
		}

		private void RenderNotification(int position, Notification notification)
		{
			bool right = corner == Corner.BottomRight || corner == Corner.TopRight;
			bool bottom = corner == Corner.BottomRight || corner == Corner.BottomLeft;

			float boxX = right ? Screen.width - BoxSize.x : 0;
			// Bottom corners stack upwards from the bottom edge; top corners stack downwards from the top edge.
			float boxY = bottom
				? Screen.height - (int)(BoxSize.y * (position + 1))
				: (int)(BoxSize.y * position);

			GUI.Box(new Rect(boxX, boxY, BoxSize.x, BoxSize.y), notification.title);

			GUI.Label(new Rect(boxX + BoxContentPadding.x, boxY + BoxHeaderSize.y, BoxContentSize.x, BoxContentSize.y), notification.message);

			GUI.HorizontalSlider(new Rect(boxX, boxY + BoxHeaderSize.y + BoxContentSize.y, BoxSize.x, BoxSize.y), notification.ttl - notification.lifetime, 0, notification.ttl);
		}

		// The most notifications that physically fit stacked in half the screen height.
		public int GetMaxNotifications()
		{
			return Screen.height / 2 / (int)BoxSize.y;
		}

		// The number of notifications actually shown: the user's cap, but never more than physically fit.
		public int GetEffectiveMax()
		{
			return Math.Max(1, Math.Min(GetMaxNotifications(), maxNotifications));
		}

		// The time to live value for a notification should be five seconds if it is a success message, and ten seconds if it is a failure message
		public void Send(string title, string message, float ttl = 10)
		{
			Hydra.Log.LogMessage($"[Notification] [{title}] {message}");

			if(disableNotifications) return;

			Notification notification = new Notification(title, message, ttl);
			notifications.Add(notification);
		}

		public void ClearNotifications()
		{
			notifications.Clear();
		}
	}
}