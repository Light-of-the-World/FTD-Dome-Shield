using AdvShields.Behaviours;
using UnityEngine;

namespace AdvShields.Models
{
    public static class StaticStorage
    {
        public static GameObject ShieldDomeObject { get; set; }

        public static GameObject HitEffectObject { get; set; }

        public static void LoadAsset()
        {
            AssetBundle bundle = AssetBundle.LoadFromMemory(Properties.Resources.shielddome);

            GameObject objShield = bundle.LoadAsset<GameObject>("assets/external/BasicShield.prefab");
            objShield.AddComponent<ShieldDomeBehaviour>();
            ShieldDomeObject = objShield;

            GameObject objEffect = bundle.LoadAsset<GameObject>("assets/external/BasicShieldHitEffect.prefab");
            objEffect.AddComponent<HitEffectBehaviour>();
            HitEffectObject = objEffect;
        }
    }
}
