using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrilliantSkies.Common.Controls.AdvStimulii;
using BrilliantSkies.Ui.Consoles;
using UnityEngine;
using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.Widgets;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Consoles.Styles;
using BrilliantSkies.Ui.Displayer;
using BrilliantSkies.Ui.Displayer.Types;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.UiSounds;
using BrilliantSkies.Core.Unity;
using BrilliantSkies.Ftd.LearningMaterial.LearningHelp;
using BrilliantSkies.Localisation;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Ui.Consoles.BackgroundDisplays;
using BrilliantSkies.Ui.Elements;
using BrilliantSkies.Ui.Layouts;
using BrilliantSkies.Ui.Tips;

namespace AdvShields.UI
{
    public class AdvShieldUi : ConsoleUi<AdvShieldProjector>
    {
        public AdvShieldUi(AdvShields.AdvShieldProjector focus) : base(focus)
        {
        }

        protected override ConsoleWindow BuildInterface(string suggestedName = "")
        {
            ConsoleWindow window = this.NewWindow(0,"Shield Dome", new ScaledRectangle(10f, 10f, 550f, 780f));
            window.DisplayTextPrompt = false;
            window.SetMultipleTabs(new AdvShieldTab(window, _focus), new AdvShieldAppearanceTab(window, _focus), new ControlUiTab(window, _focus.Control, "Shield drive complex controller settings"));
            return window;

        }
    }
}
