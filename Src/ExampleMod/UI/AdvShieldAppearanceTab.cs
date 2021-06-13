using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;
using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Builders;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters;
using BrilliantSkies.Ui.Consoles.Interpretters.Simple;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Buttons;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Numbers;
using BrilliantSkies.Ui.Consoles.Segments;
using BrilliantSkies.Ui.Layouts.DropDowns;
using BrilliantSkies.Ui.Tips;
using UnityEngine;
using AdvShields.Models;

namespace AdvShields.UI
{ 
    public class AdvShieldAppearanceTab : SuperScreen<AdvShieldProjector>
    {
        public AdvShieldAppearanceTab(ConsoleWindow window, AdvShieldProjector focus)
            : base(window, focus)
        {
            Name = new Content("Shield Dome Appearance", new ToolTip("Adjust the appearance of the shield dome", 200f), "shieldw");
        }

        public override void Build()
        {
            ScreenSegmentStandard standardSegment1 = CreateStandardSegment(InsertPosition.OnCursor);
            
            var data = _focus.VisualData;
            
            StringDisplay stringDisplay2 = standardSegment1.AddInterpretter(StringDisplay.Quick("<i>Select passive appearance of the shield:</i>"));

            standardSegment1.AddInterpretter(Quick.SliderNub(data, t => "Edge", null));
            standardSegment1.AddInterpretter(Quick.SliderNub(data, t => "Fresnel", null));
            standardSegment1.AddInterpretter(Quick.SliderNub(data, t => "AssembleSpeed", null));
            standardSegment1.AddInterpretter(new Blank(30f));

            StringDisplay stringDisplay3 = standardSegment1.AddInterpretter(StringDisplay.Quick("<i>Select animated appearance of the shield:</i>"));
            standardSegment1.AddInterpretter(Quick.SliderNub(data, t => "SinWaveFactor", null));
            standardSegment1.AddInterpretter(Quick.SliderNub(data, t => "SinWaveSpeed", null));
            standardSegment1.AddInterpretter(Quick.SliderNub(data, t => "SinWaveSize", null));
            standardSegment1.AddInterpretter(new Blank(30f));
            standardSegment1.SpaceBelow = 40f;
            standardSegment1.SpaceAbove = 40f;
            stringDisplay2.Justify = new TextAnchor?(TextAnchor.UpperLeft);
            stringDisplay3.Justify = new TextAnchor?(TextAnchor.UpperLeft);


            new ColorBuilder(CreateStandardSegment(InsertPosition.OnCursor)).RgbAdjust(data.BaseColor, true);
            ScreenSegmentStandardHorizontal standardHorizontal = CreateStandardHorizontalSegment();
            var baseColorPreview = new SubjectiveColorDisplay<AdvShieldData>(_focus.ShieldData, M.m<AdvShieldData>("Base Color"), M.m<AdvShieldData>(new ToolTip("The color of the shield", 200f)), M.m<AdvShieldData>(I => (Color)(Var<Color>)I.BaseColor))
            {
                PrescribedHeight = new PixelSizing(60f, Dimension.Height)
            };
            standardHorizontal.AddInterpretter(baseColorPreview);

            standardSegment1.AddInterpretter(new Blank(30f));

            new ColorBuilder(CreateStandardSegment(InsertPosition.OnCursor)).RgbAdjust(data.GridColor, true);
            ScreenSegmentStandardHorizontal standardHorizontal2 = CreateStandardHorizontalSegment();
            var gridColorPreview = new SubjectiveColorDisplay<AdvShieldData>(_focus.ShieldData, M.m<AdvShieldData>("Grid Color"), M.m<AdvShieldData>(new ToolTip("The color of the shield", 200f)), M.m<AdvShieldData>(I => (Color)(Var<Color>)I.GridColor))
            {
                PrescribedHeight = new PixelSizing(60f, Dimension.Height)
            };
            standardHorizontal2.AddInterpretter(gridColorPreview);

            CreateSpace(0);

            ScreenSegmentStandardHorizontal horizontalSegment2 = CreateStandardHorizontalSegment();
            horizontalSegment2.SpaceBelow = 30f;
            horizontalSegment2.AddInterpretter(SubjectiveButton<AdvShieldProjector>.Quick(_focus, "Copy to clipboard", new ToolTip("Copy the shield settings to the clipboard", 200f), I => CopyPaster.Copy(I.VisualData)));
            horizontalSegment2.AddInterpretter(SubjectiveButton<AdvShieldProjector>.Quick(_focus, "Paste from clipboard", new ToolTip("Paste shield settings from the clipboard", 200f), I => CopyPaster.Paste(I.VisualData))).FadeOut = M.m((Func<AdvShieldProjector, bool>)(I => !CopyPaster.ReadyToPaste(I.ShieldData)));
        }
    }
}
