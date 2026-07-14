using UnityEngine;

namespace HydraMenu.features
{
    public class VisualsRenderer : MonoBehaviour
    {
        private float _lastUpdateTime = 0f;
        private const float UpdateInterval = 0.1f;

        private void Update()
        {
            if (!Visuals.ShowKillCooldown && !Visuals.ShowImpostors && !Visuals.RevealRoles) return;

            if (Time.time - _lastUpdateTime < UpdateInterval) return;
            _lastUpdateTime = Time.time;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null) continue;

                string nameText = player.Data.PlayerName;
                string color = "white";
                bool isImpostor = player.Data.Role != null && player.Data.Role.CanUseKillButton;

                if (Visuals.RevealRoles && !player.Data.IsDead)
                {
                    nameText += $"\n[{player.Data.RoleType}]";
                }

                if (Visuals.ShowImpostors && isImpostor && !player.Data.IsDead)
                {
                    color = "red";
                }

                if (Visuals.ShowKillCooldown && isImpostor && !player.Data.IsDead)
                {
                    float cooldown = player.killTimer;
                    string cdColor = cooldown < 2.0f ? "red" : (cooldown < 5.0f ? "yellow" : "white");
                    nameText += $"\n<size=80%><color=\"{cdColor}\">[Cooldown: {cooldown:F1}s]</color></size>";
                }

                if (player.cosmetics != null && player.cosmetics.nameText != null)
                {
                    player.cosmetics.nameText.text = $"<color=\"{color}\">{nameText}</color>";
                }
            }
        }
    }
}
