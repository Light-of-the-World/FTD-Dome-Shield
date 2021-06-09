using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;

namespace AdvShields.Models
{
    public class ShaderColorProperty : VarColor
    {
        private Material _material;
        private string _propertyName;
        private Color _value;

        public override Color Us
        {
            get => _value;
            set
            {
                _value = value;
                _material.SetColor(_propertyName, _value);
            }
        }

        public ShaderColorProperty(Material material, string name, Color initial) : base(initial)
        {
            _material = material;
            _propertyName = name;
            _value = initial;
        }
    }
}
