using AdvShields.Models;
using BrilliantSkies.Core.Threading;
using BrilliantSkies.Ftd.DamageModels;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvShields
{
    public class AdvShieldHandler : IDamageable
    {
        public const float WaitTime = 30.0f;

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
            return CurrentDamageSustained;
        }

        public AllConstruct GetC()
        {
            return controller.GetConstructableOrSubConstructable() as AllConstruct;
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

            AdvShieldDomeData stats = GetDomeStats();
            float ac = stats.ArmorClass;
            float energy = stats.Energy;

            float damage = DD.CalculateDamage(ac, stats.GetCurrentHealth(CurrentDamageSustained), controller.GameWorldPosition);
            CurrentDamageSustained += stats.GetFactoredDamage(damage);

            //Console.WriteLine("\nDD type : " + DD.GetType().ToString() + "\nDamege : " + damage);

            //float magnitude = expDD == null ? damage / 300 : expDD.Radius;
            //Vector3 hitPosition = (expDD == null ? GridcastHit : expDD.Position) - Controller.GameWorldPosition;

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

            if (CurrentDamageSustained >= energy)
            {
                CurrentDamageSustained = energy;
                controller.ShieldData.Type.Us = enumShieldDomeState.Off;
            }

            float remainingHealthFraction = Mathf.Clamp01((energy - CurrentDamageSustained) / energy);
            Color hitColor = Color.Lerp(Color.red, Color.green, remainingHealthFraction);
            controller.ShieldDome.CreateAnimation(hitPosition, Mathf.Max(magnitude, 1), hitColor);
        }

        public AdvShieldDomeData GetDomeStats()
        {
            Shape.UpdateInfo();
            return new AdvShieldDomeData(controller, Shape.SurfaceArea());
        }

        public void Update()
        {
            if (Time.time - TimeSinceLastHit < WaitTime) return;
            if (CurrentDamageSustained == 0.0f) return;

            LaserNode laserNode = controller.ConnectLaserNode;

            if (laserNode == null) return;
            if (laserNode.HasToWaitForCharge()) return;

            LaserRequestReturn request = laserNode.GetPulsedEnergyAvailable(true);
            CurrentDamageSustained -= request.Energy;

            if (CurrentDamageSustained <= 0)
            {
                controller.ShieldData.Type.Us = enumShieldDomeState.On;
                CurrentDamageSustained = 0.0f;
            }
        }
    }

    public struct AdvShieldDomeData
    {
        public float Energy { get; set; }

        public float ArmorClass { get; set; }

        public float SurfaceFactor { get; set; }

        public float MaxHealth { get; set; }

        public AdvShieldDomeData(AdvShieldProjector controller, float surface)
        {
            Energy = 0;
            ArmorClass = 0; 

            if (controller.ConnectLaserNode != null)
            {
                Energy = controller.ConnectLaserNode.GetTotalEnergyAvailable();
            }

            LaserNode laser = controller.ConnectLaserNode;

            if (laser != null)
            {
                LaserRequestReturn request = laser.GetPulsedEnergyAvailable(false);
                float energyForLaser= laser.GetTotalEnergyAvailable();

                ArmorClass += request.AP * 0.5f * (energyForLaser / Energy);
            }


            SurfaceFactor = 1; //surface / AdvShieldDome.BaseSurface;
            MaxHealth = Energy / SurfaceFactor;
        }
       
        public float GetCurrentHealth(float sustainedUnfactoredDamage)
        {
            return (Energy - sustainedUnfactoredDamage) / SurfaceFactor;
        }

        public float GetFactoredDamage(float unfactoredDamage)
        {
            return unfactoredDamage * SurfaceFactor;
        }
    }//removed *2
}