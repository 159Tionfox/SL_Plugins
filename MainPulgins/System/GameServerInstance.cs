using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Collections.Generic;

namespace MainPlugins
{
    public static class GameServerInstance
    {

        public static void Register(bool enable)
        {
            if (enable)
            {
                Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
            }
            else
            {
                Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
            }
        }

        private static void OnPlayerVerified(VerifiedEventArgs ev)
        {
            Timing.RunCoroutine(DelayedWelcomeMessage(ev.Player));
        }

        private static IEnumerator<float> DelayedWelcomeMessage(Player player)
        {
            yield return Timing.WaitForSeconds(2.0f);
            Map.Broadcast(3, $"\n<size=75%>※欢迎 {player.Nickname} 加入服务器！</size>", Broadcast.BroadcastFlags.Normal, false);
        }
    }
}