using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;
namespace AdvShields.Models
{
    public class ShaderFloatProperty : VarFloatClamp
    {
        private Material _material;
        private string _propertyName;
        private float _value;

        public override float Us
        {
            get => _value;
            set
            {
                _value = value;
                _material.SetFloat(_propertyName, _value);
            }
        }

        public ShaderFloatProperty(Material material, string name, float initial, float min, float max, NoLimitMode noLimit) : base(initial, min, max, noLimit)
        {
            _material = material;
            _propertyName = name;
            _value = initial;
        }
    }
}
