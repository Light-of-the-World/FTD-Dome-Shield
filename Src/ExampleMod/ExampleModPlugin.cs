using System;
using System.Reflection;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Collections;
using BrilliantSkies.Modding;
using BrilliantSkies.Modding.Containers;
using BrilliantSkies.Modding.Types;
using BrilliantSkies.Ui.Layouts;
using BrilliantSkies.Ui.Tips;
using Harmony;
using UnityEngine;
using System.Linq;
using AdvShields.Models;

namespace AdvShields
{

    /// <summary>
    /// All code files using the GamePlugin or GamePlugin_PostLoad interfaces (no need to use both)
    /// will have their OnLoad method called when they are loaded by the plugin loader.
    /// </summary>
    public class ExampleModPlugin : GamePlugin
    {
        public void OnLoad()
        {
            var harmony = HarmonyInstance.Create("com.BarrelRenderPatch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }

        /// <summary>
        /// Not currently called from anywhere in FTD.
        /// </summary>
        public void OnSave()
        {
        }

        /// <summary>
        /// Not directly used in FTD.
        /// </summary>
        public string name
        {
            get { return "Advanced Shields"; }
        }

        /// <summary>
        /// Don't worry about this- it's not used.
        /// </summary>
        public Version version
        {
            get { return new Version(0, 0, 1); }
        }


        ///// <summary>
        ///// Used if using GamePlugin_PostLoad interface.
        ///// </summary>
        ///// <returns></returns>
        //public bool AfterAllPluginsLoaded()
        //{
        //    SafeLogging.Log("Called after all other mods loaded... Extra Hatches");
        //    //ModdingEvents.AddYourModules += HookUp;
        //    return true;
        //}

        //private void HookUp(IDictionaryOfTypedTypes<IComponentContainer> typecontainer, string directory)
        //{
        //    // Add our ModWidgets container to all mods / configurations from now on.
        //    typecontainer.Add(new ModWidgets(directory));
        //}


    }


    //public class ModWidgets : AbstractContainer<ModWidget>
    //{
    //    public ModWidgets(string directoryWithTrailingSlash) : base(directoryWithTrailingSlash)
    //    {
    //        UiContent = new Ui.Consoles.Content("Widget example",
    //            new ToolTip(
    //                "Added by ExampleMod. Used to demonstrate how to add your own categories of object via mods"));

    //    }

    //    public override IModComponent AddNew()
    //    {
    //        var newOne = new ModWidget();
    //        Components.Add(newOne);
    //        return newOne;
    //    }

    //    protected override string DirName => "MyWidgets";

    //    protected override string FileExtension => ".myWidget";
    //}
    
    //public class ModWidget : ModComponentAbstract
    //{
    //    public string OurUselessString { get; set; } = "Useless string";

    //    public override void GuiEditor()
    //    {
    //        base.GuiEditor();
    //       OurUselessString =  MapEditorGuiCommon.StringEditor("Useless string example", OurUselessString,
    //            new Ui.Tips.ToolTip("Just an example"));
    //    }
    //}

}
