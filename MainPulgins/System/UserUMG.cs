using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace MainPlugins.System
{
    public static class UserUMG
    {
        private static readonly Dictionary<string, CoroutineHandle> _handles = new Dictionary<string, CoroutineHandle>();

        public static void Register(bool enable)
        {
            if (enable)
            {
                Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
                Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
            }
            else
            {
                Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
            }
        }

        private static void OnPlayerVerified(VerifiedEventArgs ev)
        {
            StartShow(ev.Player);
        }

        private static void OnPlayerLeft(LeftEventArgs ev)
        {
            StopShow(ev.Player);
        }

        public static void StartShow(Player player)
        {
            if (_handles.ContainsKey(player.UserId))
                Timing.KillCoroutines(_handles[player.UserId]);

            _handles[player.UserId] = Timing.RunCoroutine(ShowInfoCoroutine(player));
        }

        public static void StopShow(Player player)
        {
            if (_handles.TryGetValue(player.UserId, out var handle))
            {
                Timing.KillCoroutines(handle);
                _handles.Remove(player.UserId);
            }
        }



        private static IEnumerator<float> ShowInfoCoroutine(Player player)
        {
            while (player != null && player.IsConnected)
            {
                string roleType = GetRoleTypeString(player);
                float tps = Time.smoothDeltaTime > 0 ? 1f / Time.smoothDeltaTime : 0f;

                // 使用 Hint 并指定底部居中效果
                string msg = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=50%><b>{player.Nickname}</b> | <color=yellow>{roleType}</color> | TPS: {tps:F1}</size>";
                player.ShowHint(msg, 1.1f);

                yield return Timing.WaitForSeconds(1f);
            }
        }

        private static string GetRoleTypeString(Player player)
        {
            if (Exiled.API.Features.Round.IsLobby)
                return "大厅等待中";

            if (CharacterBase.Allscps.TryGetValue(player.UserId, out var customChar))
            {
                var type = customChar.GetType();
                var scpNumberProp = type.GetProperty("ScpNumber");
                if (scpNumberProp != null)
                {
                    var scpNumber = scpNumberProp.GetValue(customChar) as string[];
                    if (scpNumber != null && scpNumber.Length > 1)
                        return $"SCP-{scpNumber[1]}";
                }
                return $"自定义角色 {customChar.CIAttribute?.Name ?? "未知"}";
            }

            return $"官方角色 {player.Role.Type}";
        }
    }
}