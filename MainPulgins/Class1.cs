using Exiled.API.Features;
using Exiled.API.Interfaces;
using MainPlugins.System;
using System.ComponentModel;

namespace MainPlugins
{
    public class PluginConfig : IConfig
    {
        [Description("是否启用插件")]
        public bool IsEnabled { get; set; } = true;

        [Description("是否启用调试模式")]
        public bool Debug { get; set; } = false;
    }

    public class MainPlugins : Plugin<PluginConfig>
    {
        public override void OnEnabled()
        {
            base.OnEnabled();
            GameServerInstance.Register(true);
            CharacterBase.RegisterAll(true);
            UserUMG.Register(true);
        }
    }
}