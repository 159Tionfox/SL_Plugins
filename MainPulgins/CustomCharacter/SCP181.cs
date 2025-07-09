using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;

namespace MainPlugins
{
    [CharacterInfo(Side.ChaosInsurgency, "SCP-181", MaxHealth = 120, istimer = false)]
    public class SCP181 : CharacterBase
    {
        public SCP181(string uid) : base(uid) { }
        public override string[] ScpNumber => new[] { "1 8 1", "181"};

        public override void Register(bool enable)
        {
            if (enable)
            {
                Exiled.Events.Handlers.Player.InteractingDoor += P_InteractingDoor;
                Exiled.Events.Handlers.Player.InteractingLocker += P_InteractingLocker;
            }
            else
            {
                Exiled.Events.Handlers.Player.InteractingDoor -= P_InteractingDoor;
                Exiled.Events.Handlers.Player.InteractingLocker -= P_InteractingLocker;
            }
        }

        private static void P_InteractingDoor(InteractingDoorEventArgs ev)
        {
            if (Allscps.ContainsKey(ev.Player.UserId) && Allscps[ev.Player.UserId] is SCP181)
            {
                if (UnityEngine.Random.Range(0, 5) >= 4)
                {
                    ev.IsAllowed = true;
                }
            }
        }

        private static void P_InteractingLocker(InteractingLockerEventArgs ev)
        {
            if (Allscps.ContainsKey(ev.Player.UserId) && Allscps[ev.Player.UserId] is SCP181)
            {
                if (UnityEngine.Random.Range(0, 5) == 5)
                {
                    ev.IsAllowed = true;
                }
            }
        }
    }
}