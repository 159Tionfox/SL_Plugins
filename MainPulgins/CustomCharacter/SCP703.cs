using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MainPlugins
{
    [CharacterInfo(Side.ChaosInsurgency, "SCP-703", MaxHealth = 100, istimer = true)]
    public class SCP703 : CharacterBase
    {
        public SCP703(string uid) : base(uid) { }
        public override string[] ScpNumber => new[] { "7 0 3", "703" };

        public override IEnumerator<float> ServerTimeEvent()
        {
            while (true)
            {
                if (player == null || !player.IsAlive) yield break;

                GiveItem();
                yield return Timing.WaitForSeconds(100.0f);
            }
        }

        private void GiveItem()
        {
            if (player == null || !player.IsAlive || player.Items.Count >= 8)
                return;

            var validItems = Enum.GetValues(typeof(ItemType)).Cast<ItemType>().Where(t => (int)t > 0 && t != ItemType.None).ToList();

            ItemType randomType = validItems[UnityEngine.Random.Range(0, validItems.Count)];
            Item item = Item.Create(randomType);
            if (item is Armor armor)
            {
                player.AddItem(item);
            }
            else
            {
                item.Give(player);
            }
        }

    }
}