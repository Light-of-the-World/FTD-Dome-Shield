using AdvShields.Models;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;
using UnityEngine;

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

        private bool isActive;

        private ShaderFloatProperty progress;

        private float timeToShift = 4;

        public void Update()
        {
            if (isActive && progress < 1)
            {
                progress.Us = Mathf.Clamp01(progress + Time.deltaTime / timeToShift);
            }
            else if (!isActive && progress > 0)
            {
                progress.Us = Mathf.Clamp01(progress - Time.deltaTime / timeToShift);
            }
        }

        public void Initialize()
        {
            Material material = gameObject.GetComponent<MeshRenderer>().material;
            progress = new ShaderFloatProperty(material, "_Progress", 0, 0, 1, NoLimitMode.None);
        }

        public void UpdateSizeInfo(AdvShieldData data)
        {
            transform.localScale = new Vector3(data.Width, data.Height, data.Length);
        }

        public bool SetState(bool isActive)
        {
            if (this.isActive == isActive) return false;

            this.isActive = isActive;
            return true;
        }

        public void CreateAnimation(Vector3 worldHit, float magnitude, Color color)
        {
            GameObject obj = Instantiate(StaticStorage.HitEffectObject, transform, false);
            HitEffectBehaviour behaviour = obj.GetComponent<HitEffectBehaviour>();
            behaviour.Initialize(worldHit, color, magnitude, 1.5f);
        }
    }
}
