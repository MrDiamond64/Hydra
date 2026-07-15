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
                    nameText += $" <size=60%>[{player.Data.RoleType}]</size>";
                }

                if (Visuals.ShowImpostors && isImpostor && !player.Data.IsDead)
                {
                    color = "red";
                }

                if (player.cosmetics != null && player.cosmetics.nameText != null)
                {
                    string displayText = $"<color=\"{color}\">{nameText}</color>";

                    if (Visuals.ShowKillCooldown && isImpostor && !player.Data.IsDead)
                    {
                        float cooldown = player.killTimer;
                        string cdColor = cooldown < 2.0f ? "red" : (cooldown < 5.0f ? "yellow" : "white");
                        displayText = $"<size=80%><color=\"{cdColor}\">[Cooldown: {cooldown:F1}s]</color></size>\n{displayText}";
                    }

                    player.cosmetics.nameText.text = displayText;
                }
            }
        }
    }
}
