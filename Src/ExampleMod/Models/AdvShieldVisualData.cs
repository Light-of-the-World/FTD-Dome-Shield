using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;
using BrilliantSkies.Core.Widgets;
using UnityEngine;

namespace AdvShields.Models
{
    public class AdvShieldVisualData : PrototypeSystem
    {
        [Slider(0, "Edge", "Makes the grid color more intense", 0, 100, 0.05f)]
        public VarFloatClamp Edge { get; set; }
        
        [Slider(1, "Fresnel", "A higher value increases center transparency", 0, 100, 0.05f)]
        public VarFloatClamp Fresnel { get; set; }

        [Slider(2, "Assemble Speed", "Makes the grid color more intense", 0, 3, 0.01f)]
        public VarFloatClamp AssembleSpeed { get; set; }

        [Slider(3, "Wave Factor", "Makes the grid color more intense", 0, 100, 0.05f)]
        public VarFloatClamp SinWaveFactor { get; set; }

        [Slider(4, "Wave Speed", "Makes the grid color more intense", 0, 100, 0.05f)]
        public VarFloatClamp SinWaveSpeed { get; set; }

        [Slider(5, "Wave Size", "Makes the grid color more intense", 0, 100, 0.05f)]
        public VarFloatClamp SinWaveSize { get; set; }

        [Variable(6, "Base Color", "Makes the grid color more intense")]
        public VarColor BaseColor { get; set; }

        [Variable(7, "Grid Color", "Makes the grid color more intense")]
        public VarColor GridColor { get; set; }



        public AdvShieldVisualData(uint uniqueId) : base(uniqueId)
        {
            //Expansion = new ShaderFloatProperty(_material, "_Expansion");
            //NormalSwitch = new ShaderFloatProperty(_material, "_NormalSwitch");
            Edge = new VarFloatClamp(9, 0, 100, NoLimitMode.Max);
            Fresnel = new VarFloatClamp(3, 0, 100, NoLimitMode.Max);

            SinWaveFactor = new VarFloatClamp(1, 0, 100, NoLimitMode.Max);
            SinWaveSpeed = new VarFloatClamp(0.5f, 0, 100, NoLimitMode.Max);
            SinWaveSize = new VarFloatClamp(0.2f, 0, 100, NoLimitMode.Max);

            AssembleSpeed = new VarFloatClamp(0.25f, 0, 3, NoLimitMode.None);


            BaseColor = new VarColor(Color.blue);
            GridColor = new VarColor(Color.yellow);
        }


    }
}
