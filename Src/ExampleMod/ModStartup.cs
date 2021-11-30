using AdvShields.Models;
using HarmonyLib;
using System.Reflection;

namespace ModManagement
{
    public static class ModStartup
    {
        /*
        public static void OnLoad()
        {
        }
        */

        public static void OnStart()
        {
            StaticStorage.LoadAsset();

            Harmony harmony = new Harmony("AdvShields_Patch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        /*
        public static void OnSave()
        {
        }
        */
    }
}
