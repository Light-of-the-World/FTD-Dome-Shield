using AdvShields.Behaviours;
using AdvShields.Models;
using BrilliantSkies.Core.Threading;
using BrilliantSkies.Ftd.DamageModels;
using HarmonyLib;
using UnityEngine;

namespace AdvShields
{
    public class AdvShieldHandler : IDamageable
    {
        public const float BaseWaitTime = 48.0f;

        public const float BaseSurface = 1256;

        private AdvShieldProjector controller;

        public Rigidbody Rigidbody => controller.MainConstruct.PlatformPhysicsRestricted.GetRigidbody();

        public Transform transform => controller.GetConstructableOrSubConstructable().GameObject.myTransform;

        public bool RequireRaycastForExplosion => true;

        public float TimeSinceLastHit { get; private set; }

        public float CurrentDamageSustained { get; set; }

        public Elipse Shape { get; set; }

        public Vector3 GridcastHit { get; set; }

        public float GetCurrentHealth()
        {
            return controller.ShieldStats.MaxEnergy - CurrentDamageSustained;
        }

        public AllConstruct GetC()
        {
            return controller.GetC();
        }

        public AdvShieldHandler(AdvShieldProjector controller)
        {
            this.controller = controller;
            Shape = new Elipse(controller);
        }

        [ExtraThread("Should be callable from extra thread")]
        public void ApplyDamage(IDamageDescription DD)
        {
            TimeSinceLastHit = Time.time;

            //Console.WriteLine(DD.GetType().ToString());

            AdvShieldStatus stats = controller.ShieldStats;

            float damage = DD.CalculateDamage(stats.ArmorClass, GetCurrentHealth(), controller.GameWorldPosition);
            CurrentDamageSustained += damage * controller.SurfaceFactor;

            float magnitude;
            Vector3 hitPosition;

            if (DD is ExplosionDamageDescription)
            {
                ExplosionDamageDescription expDD = DD as ExplosionDamageDescription;
                magnitude = expDD.Radius;
                hitPosition = expDD.Position - controller.GameWorldPosition;
            }
            else if (DD is ApplyDamageCallback)
            {
                ApplyDamageCallback adc = (ApplyDamageCallback)DD;
                magnitude = Traverse.Create(adc).Field("_radius").GetValue<float>();
                hitPosition = GridcastHit - controller.GameWorldPosition;
            }
            else
            {
                magnitude = damage / 300;
                hitPosition = GridcastHit - controller.GameWorldPosition;
            }

            float maxEnergy = stats.MaxEnergy;

            if (CurrentDamageSustained >= maxEnergy)
            {
                CurrentDamageSustained = maxEnergy;
                controller.ShieldData.Type.Us = enumShieldDomeState.Off;
            }

            float remainingHealthFraction = Mathf.Clamp01((maxEnergy - CurrentDamageSustained) / maxEnergy);
            Color hitColor = Color.Lerp(Color.red, Color.green, remainingHealthFraction);
            CreateAnimation(hitPosition, Mathf.Max(magnitude, 1), hitColor);
        }

        public void CreateAnimation(Vector3 worldHit, float magnitude, Color color)
        {
            GameObject obj = UnityEngine.Object.Instantiate(StaticStorage.HitEffectObject, controller.ShieldDome.transform, false);
            HitEffectBehaviour behaviour = obj.GetComponent<HitEffectBehaviour>();
            behaviour.Initialize(worldHit, color, magnitude, 1.5f);
        }

        public void Update(AdvShieldStatus ShieldStats)
        {
            if (Time.time - TimeSinceLastHit < ShieldStats.WaitTime) return;
            if (CurrentDamageSustained == 0.0f) return;

            LaserNode laserNode = controller.ConnectLaserNode;

            if (laserNode == null) return;
            //            if (laserNode.HasToWaitForCharge()) return;
            if ((CurrentDamageSustained / ShieldStats.MaxEnergy <= 0.4f)&&(controller.ShieldData.Type.Us ==enumShieldDomeState.Off))
            {
                controller.ShieldData.Type.Us = enumShieldDomeState.On;
                CurrentDamageSustained = ShieldStats.MaxEnergy * 0.4f;//+100;
            }
            //Added this^^
            LaserRequestReturn continuousReturn = laserNode.GetCWEnergyAvailable(true);
            LaserRequestReturn pulsedReturn = laserNode.GetPulsedEnergyAvailable(true);

            if /*((CurrentDamageSustained>0.0f)&&(Time.time - TimeSinceLastHit < ShieldStats.WaitTime))*/(continuousReturn.WorthFiring)
            {
                CurrentDamageSustained -= continuousReturn.Energy;
            }

            if /*((CurrentDamageSustained > 0.0f)&&(Time.time - TimeSinceLastHit < ShieldStats.WaitTime))*/(pulsedReturn.WorthFiring)
            {
                CurrentDamageSustained -= pulsedReturn.Energy;
            }

            if (CurrentDamageSustained <= 0)
            {
                controller.ShieldData.Type.Us = enumShieldDomeState.On;
                CurrentDamageSustained = 0.0f;
            }
        }
    }
}