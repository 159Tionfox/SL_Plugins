using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MainPlugins
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CharacterInfoAttribute : Attribute
    {
        public CharacterInfoAttribute(Side side, string name)
        {
            TeamSide = side;
            Name = name;
        }
        public Side TeamSide { get; }
        public string Name { get; }
        public int MaxHealth { get; set; }
        public bool istimer { get;set; }
    }

    public abstract class CharacterBase
    {
        private static readonly HashSet<string> _selectedPlayers = new HashSet<string>();
        private CoroutineHandle _timer;

        public abstract string[] ScpNumber { get; }

        public CharacterBase(string uid)
        {
            UserId = uid;
        }

        public static Dictionary<string, CharacterBase> Allscps = new Dictionary<string, CharacterBase>();
        public string UserId { get; }
        public Player player => Player.Get(UserId);
        public CharacterInfoAttribute CIAttribute => this.GetType().GetCustomAttribute<CharacterInfoAttribute>();

        public virtual void OnSpawn()
        {
            Register(true);
        }

        public static void SpawnPlayer<T>(Player player) where T : CharacterBase
        {
            SpawnPlayer(typeof(T), player);
        }

        public static void SpawnPlayer(Type type, Player player)
        {
            CharacterBase pc = (CharacterBase)Activator.CreateInstance(type, new string[] { player.UserId });
            pc.OnSpawn();
            player.MaxHealth = pc.CIAttribute.MaxHealth;
            player.Health = pc.CIAttribute.MaxHealth;
            if (pc.CIAttribute.istimer)
            {
                pc._timer = Timing.RunCoroutine(pc.ServerTimeEvent());
            }
            else
            {
                Timing.KillCoroutines(pc._timer);
            }
            Allscps.Add(player.UserId, pc);
        }

        public virtual IEnumerator<float> ServerTimeEvent()
        {
            yield break;
        }

        public virtual void Register(bool enable) { }
        public virtual void Inner_Died() { }

        public static void RegisterAll(bool enable)
        {
            if (enable)
            {
                Exiled.Events.Handlers.Player.Died += P_Died;
                Exiled.Events.Handlers.Server.RoundStarted += S_RoundStarted;
                Exiled.Events.Handlers.Server.RoundEnded += S_RoundEnded;
            }
            else
            {
                Exiled.Events.Handlers.Player.Died -= P_Died;
                Exiled.Events.Handlers.Server.RoundStarted -= S_RoundStarted;
                Exiled.Events.Handlers.Server.RoundEnded -= S_RoundEnded;
            }
        }

        private static void P_Died(DiedEventArgs ev)
        {
            if (Allscps.ContainsKey(ev.Player.UserId))
            {
                var a = Allscps[ev.Player.UserId];
                GetTeam(ev.Attacker.Role.Team, a.ScpNumber,ev.Attacker);
                a.Inner_Died();
                a.Register(false);
                Allscps.Remove(ev.Player.UserId);
            }
        }

        private static void GetTeam(Team team,string[] number, Player p)
        {
            switch(team)
            {
                case Team.FoundationForces:
                    CASSIE($"SCP-{number[1]}  已被 {p.UnitName} 收容。", $"SCP .G1 {number[0]} .G3 contained successfully. Containment unit {p.UnitName}.");
                    break;
                case Team.ChaosInsurgency:
                    CASSIE($"SCP-{number[1]}  已被混沌分裂者收容。", $"SCP .G1 {number[0]} .G3 contained successfully by Chaos Insurgency.");
                    break;
                case Team.SCPs:
                    CASSIE($"SCP-{number[1]} 已被 {p.Role.ToString()} 收容。", $"SCP .G1 {number[0]} .G3 terminated by {p.Role.ToString()}.");
                    break;
                case Team.ClassD:
                    CASSIE($"SCP-{number[1]} 已被D级人员收容。", $"SCP .G1 {number[0]} .G3 contained successfully by Class D Personnel");
                    break;
                case Team.Scientists:
                    CASSIE($"SCP-{number[1]} 已被科学家收容。", $"SCP .G1 {number[0]} .G3 contained successfully by Scientists");
                    break;
                default:
                    CASSIE($"SCP-{number[1]} 已被收容。死亡原因不明。", $"SCP .G1 {number[0]} .G3 successfully terminated. .G1 Termination cause unspecified.");
                    break;
            }
        }

        private static void CASSIE(string cnMsg,string enMsg)
        {
            Cassie.Message(enMsg);
            Cassie.Message(cnMsg, isNoisy: false, isSubtitles: true);
        }

        private static void S_RoundStarted()
        {
            _selectedPlayers.Clear();
            Allscps.Clear();

            // 1. 从科学家中选择SCP-703
            var validScientists = Player.List
                .Where(p => p.Role == RoleTypeId.Scientist && !p.IsScp && !_selectedPlayers.Contains(p.UserId))
                .ToList();

            if (validScientists.Count > 0 && !Allscps.Values.Any(scp => scp is SCP703))
            {
                Player selectedScientist = validScientists[UnityEngine.Random.Range(0, validScientists.Count)];
                _selectedPlayers.Add(selectedScientist.UserId);
                SpawnPlayer<SCP703>(selectedScientist);
                selectedScientist.ShowHint("<color=blue><b><size=75%>你已被选中为 SCP-703</size></b></color>", 5f);
            }

            // 2. 从D级人员中选择SCP-181
            var validDClass = Player.List
                .Where(p => p.Role == RoleTypeId.ClassD && !p.IsScp && !_selectedPlayers.Contains(p.UserId))
                .ToList();

            if (validDClass.Count > 0 && !Allscps.Values.Any(scp => scp is SCP181))
            {
                Player selectedDClass = validDClass[UnityEngine.Random.Range(0, validDClass.Count)];
                _selectedPlayers.Add(selectedDClass.UserId);
                SpawnPlayer<SCP181>(selectedDClass);
                selectedDClass.ShowHint("<color=red><b><size=75%>你已被选中为 SCP-181</size></b></color>", 5f);
            }
        }

        private static void S_RoundEnded(RoundEndedEventArgs ev)
        {
            foreach (var scp in Allscps.Values)
            {
                if (scp.CIAttribute.istimer)
                {
                    Timing.KillCoroutines(scp._timer);
                }
            }
            _selectedPlayers.Clear();
            Allscps.Clear();
        }
    }
}