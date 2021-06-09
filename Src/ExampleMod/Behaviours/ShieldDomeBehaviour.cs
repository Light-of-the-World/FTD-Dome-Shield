using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using AdvShields.Models;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;

namespace AdvShields.Behaviours
{
    public partial class ShieldDomeBehaviour : MonoBehaviour
    {
        //private Material _material;

        // Shader
        //public ShaderFloatProperty Metallic { get; set; }
        //public ShaderFloatProperty Gloss { get; set; }
        //public ShaderFloatProperty Expansion { get; set; }
        //public ShaderFloatProperty NormalSwitch { get; set; }

        public ShaderFloatProperty Progress { get; set; }

        public float TimeToShift { get; set; }
        public bool IsActive { get; set; }

        public void Awake()
        {
        }

        public void Initialize(Material material)
        {
            Progress = new ShaderFloatProperty(material, "_Progress", 0, 0, 1, NoLimitMode.None);
            
            TimeToShift = 4;
        }

        public void UpdateSizeInfo(AdvShieldData data)
        {
            transform.localScale = new Vector3(data.Width, data.Height, data.Length);
        }

        public void Update()
        {
            if (IsActive && Progress < 1)
                Progress.Us = Mathf.Clamp01(Progress + Time.deltaTime / TimeToShift);
            else if (!IsActive && this.Progress > 0)
                Progress.Us = Mathf.Clamp01(Progress - Time.deltaTime / TimeToShift);
        }
        

        public bool SetState(bool isActive)
        {
            if (IsActive == isActive)
                return false;

            IsActive = isActive;
            return true;
        }


        public void CreateAnimation(Vector3 worldHit, float magnitude, Color color)
        {
            var obj = Instantiate(StaticStorage.HitEffectObject, transform, false);
            var behaviour = obj.GetComponent<HitEffectBehaviour>();
            
            behaviour.Initialize(worldHit, color, magnitude, 1.5f);
        }
    }
}
