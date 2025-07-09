using CommandSystem;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MainPlugins.System
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(ClientCommandHandler))]

    public class SCommand : ICommand, IUsageProvider
    {
        private static readonly HashSet<string> _usedPlayers = new HashSet<string>();

        public string[] Aliases => Array.Empty<string>();
        public string Description => "发送全服公告（每位玩家每局只能使用一次）";
        public string[] Usage => new string[] { "消息内容" };
        public string Command => "gg";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = string.Empty;

            if (!(sender is CommandSender cmdSender)) return false;

            var potentialPlayer = Player.Get(cmdSender.SenderId);
            if (!(potentialPlayer is Player player)) return false;

            if (arguments.Count == 0 || arguments.All(string.IsNullOrWhiteSpace))
            {
                response = "<color=yellow>用法: .gg <消息内容></color>\n<size=60%>每人每局限一次</size>";
                return false;
            }

            string message = string.Join(" ", arguments.Where(arg => !string.IsNullOrWhiteSpace(arg)));

            bool isAdmin = player.Group?.Permissions > 0;

            if (!isAdmin && _usedPlayers.Contains(player.UserId))
            {
                response = "<color=red>每人每局只能发送一次公告！</color>";
                return false;
            }

            string prefix = isAdmin ?
                $"<color=#FF00FF><size=75%>[管理员]</size></color>" :
                $"<color=green><size=75%>{player.Nickname}</size></color>";

            Map.Broadcast(10, $"{prefix}: {message}", Broadcast.BroadcastFlags.Normal);

            if (!isAdmin) _usedPlayers.Add(player.UserId);
            return true;
        }

        public static void RoundStarted() => _usedPlayers.Clear();
    }
}