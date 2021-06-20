using AdvShields.Models;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;
using BrilliantSkies.Core.Widgets;

namespace AdvShields
{
    public class AdvShieldData : PrototypeSystem
    {
        public AdvShieldData(uint uniqueId) : base(uniqueId)
        {
        }

        [Slider(0, "External drive factor {0}", "This is not controlled from the UI, but from the ACB", 0.0f, 10f, 0.01f)]
        public Var<float> ExternalDriveFactor { get; set; } = new VarFloatClamp(1f, 0.0f, 10f, NoLimitMode.None);

        [Slider(1, "Length {0}m", "The length that the shield is projected", 10f, 1500f, 0.5f, 100f)]
        public Var<float> Length { get; set; } = new VarFloatClamp(15f, 10f, 1500f, NoLimitMode.None);

        [Slider(2, "Width {0}m", "The width of the shield", 10f, 1500f, 0.5f, 100f)]
        public Var<float> Width { get; set; } = new VarFloatClamp(15f, 10f, 1500f, NoLimitMode.None);

        [Slider(3, "Height {0}m", "The height of the shield", 10f, 1500f, 0.5f, 100f)]
        public Var<float> Height { get; set; } = new VarFloatClamp(15f, 10f, 1500f, NoLimitMode.None);

        [Slider(4, "Left/Right {0}", "The X position of the shield", -50f, 50f, 0.5f, 100f)]
        public Var<float> LocalPosX { get; set; } = new VarFloatClamp(0f, -50f, 50f, NoLimitMode.None);

        [Slider(5, "Up/Down {0}", "The Y position of the shield", -50f, 50f, 0.5f, 100f)]
        public Var<float> LocalPosY { get; set; } = new VarFloatClamp(0f, -50f, 50f, NoLimitMode.None);

        [Slider(6, "Forward/Back {0}", "The Z position of the shield", -50f, 50f, 0.5f, 100f)]
        public Var<float> LocalPosZ { get; set; } = new VarFloatClamp(0f, -50f, 50f, NoLimitMode.None);

        [Slider(7, "Effect strength {0}", "The strength of the effect and how much power is used for the shield", 0.0f, 10f, 0.1f, 1f)]
        public Var<float> ExcessDrive { get; set; } = new VarFloatClamp(1f, 0.0f, 10f, NoLimitMode.None);

        /*
        [Slider(8, "Azimuth angle {0}°", "The azimuth angle of the shield", -45f, 45f, 0.1f, 0.0f)]
        public Var<float> Azimuth { get; set; } = new VarFloatClamp(0.0f, -45f, 45f, NoLimitMode.None);

        [Slider(9, "Elevation angle {0}°", "The elevation angle of the shield", -45f, 45f, 0.1f, 0.0f)]
        public Var<float> Elevation { get; set; } = new VarFloatClamp(0.0f, -45f, 45f, NoLimitMode.None);
        */

        [Variable(10, "Shield type", "The type of the shield")]
        public Var<enumShieldDomeState> Type { get; set; } = new Var<enumShieldDomeState>(enumShieldDomeState.On);
    }
}
