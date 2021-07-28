using AdvShields.Models;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Modding;
using HarmonyLib;
using System;
using System.Reflection;

namespace AdvShields
{
    public class ModPlugin : GamePlugin
    {
        public string name { get; } = "Advanced Shields";

        public Version version { get; } = new Version(0, 0, 1);

        public void OnLoad()
        {
            GameEvents.StartEvent.RegWithEvent(OnStart);
        }

        public void OnStart()
        {
            GameEvents.StartEvent.UnregWithEvent(OnStart);

            Harmony harmony = new Harmony("AdvShields_Patch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            StaticStorage.LoadAsset();
        }

        public void OnSave()
        {
        }
    }
}
