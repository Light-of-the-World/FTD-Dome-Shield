using AdvShields.Models;
using BrilliantSkies.Modding;
using HarmonyLib;
using System;
using System.Reflection;

namespace AdvShields
{
    public class ExampleModPlugin : GamePlugin
    {
        public string name { get; } = "Advanced Shields";

        public Version version { get; } = new Version(0, 0, 1);

        public void OnLoad()
        {
            Harmony harmony = new Harmony("com.BarrelRenderPatch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            StaticStorage.LoadAsset();
        }

        public void OnSave()
        {
        }
    }
}
