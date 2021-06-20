using AdvShields.Models;
using BrilliantSkies.Ftd.DamageLogging;
using BrilliantSkies.Ftd.DamageModels;
using BrilliantSkies.GridCasts;
using HarmonyLib;
using System;
using UnityEngine;

namespace AdvShields
{
    [HarmonyPatch(typeof(ExplosionExtras), "ExplodeNearbyObjects", new Type[] { typeof(Vector3), typeof(float), typeof(float), typeof(IDamageLogger), typeof(bool) })]
    internal class ExplosionOnMainConstructPatch
    {
        private static void Postfix(ref Vector3 position, ref float damage, ref float radius, ref IDamageLogger gunner, ref bool damageMissiles)
        {
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.ShieldData.Type == enumShieldDomeState.Off) continue;

                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();

                if (elipse.CheckIntersection(position, radius))
                {
                    new ApplyDamageCallback(item.ShieldHandler, position, radius, damage, gunner).Enqueue();
                }
            }
        }
    }

    [HarmonyPatch(typeof(ObjectCasting), "ShieldsQuick", new Type[] { typeof(GridCastReturn), typeof(Func<ShieldProjector, bool>) })]
    internal class ShieldsQuickPatch
    {
        private static void Postfix(ref GridCastReturn results)
        {
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                AdvShieldData d = item.ShieldData;
                if (d.Type == enumShieldDomeState.Off) continue;

                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();

                bool hitSomething = elipse.CheckIntersection(results.Position, results.Direction, out Vector3 hitPointIn, out Vector3 hitNormal);
                if (!hitSomething) continue;

                float range = (results.Position - hitPointIn).magnitude;
                if (range > results.Range) continue;

                item.ShieldHandler.GridcastHit = hitPointIn;

                IAllConstructBlock allConstructBlock = item.GetConstructableOrSubConstructable();
                Vector3 hitPointInLocal = allConstructBlock.SafeGlobalToLocal(hitPointIn);

                GridCastHit hit = GridHitPool.Pool.Acquire();
                hit.Setup(hitPointInLocal, allConstructBlock.GameObject, range, HitSource.Block, results.Direction);
                hit.DamageableObject = item.ShieldHandler;
                hit.From = BarrierCondition.Unknown;

                item.PlayShieldHit(hitPointIn);
                results.AddAndSort(hit);

                //Console.WriteLine("Hit!");
            }
        }
    }

    /*
    [Serializable]
    public class AdvObjectCasting
    {
        public static void Missiles(GridCastReturn results)
        {
            int count = Objects.Instance.Missiles.Count;
            RayCuboidIntersection intersectionChecker = new RayCuboidIntersection();

            for (int index = 0; index < count; ++index)
            {
                Missile missile = Objects.Instance.Missiles.Objects[index];
                ObjectCasting.Projectile(results, missile, intersectionChecker);
            }
        }

        public static void Projectile(GridCastReturn results, IProjectile M, RayCuboidIntersection intersectionChecker)
        {
            float num = M.Diameter / 2f;
            Vector3 temp = new Vector3(num, num, M.Length / 2f);

            if (!intersectionChecker.CheckIntersection(-temp, temp, M.SafePosition, M.SafeRotation, results.Position, results.Direction, results.Range, false)) return;

            GridCastHit newHit = GridHitPool.Pool.Acquire();
            newHit.Setup(intersectionChecker.Point, M, intersectionChecker.Range, HitSource.MissileAndCram, results.Direction);
            newHit.DamageableObject = M;
            newHit.OutPointLocal = intersectionChecker.Point;
            newHit.LocalNormal = intersectionChecker.LocalNormal;
            results.AddAndSort(newHit);
        }

        public static void Explosion(ExplosionDamageDescription DD)
        {
            foreach (MainConstruct constructable in StaticConstructablesManager.constructables)
            {
                if (constructable.AABB.Contains(DD.Position, Mathf.CeilToInt(DD.Radius)))
                    ExplosionOnMainConstruct(constructable, DD);
            }
            ExplosionExtras.ExplodeNearbyObjects(DD.Position, DD.DamagePotential, DD.Radius, DD.DamageLogger, DD.DamageMissiles);
        }

        private static void ExplosionOnMainConstruct(IMainConstructBlock mainConstruct, ExplosionDamageDescription DD)
        {
            mainConstruct.ExplosionsRestricted.Explosion(DD.DeepCopy());
            new ApplyRegulatedForceCallback(mainConstruct.PlatformPhysicsRestricted.ExplosionRegulator, DD.Position, DD.Radius, DD.DamagePotential * StaticPhysics.explosionDamageToForceFactor).Enqueue();
        }

        public ICarriedObjectReference shield;

        private CarriedObjectReference shield;

        private void Shield(GridCastReturn results, AdvShieldProjector projector, RayCuboidIntersection intersectionChecker, bool forceCheck = false)
        {
            Vector3 temp = new Vector3(projector.ShieldData.Width / 2f, projector.ShieldData.Height / 2f, 0.1f);

            if (intersectionChecker.CheckIntersection(-temp, temp, shield.SafePosition, shield.SafeRotation, results.Position, results.Direction, results.Range, false))
            {
                GridCastHit newHit = GridHitPool.Pool.Acquire();
                newHit.Setup(intersectionChecker.Point, shield, intersectionChecker.Range, HitSource.Shield, results.Direction);
                newHit.BlockHit = projector;
                newHit.DamageableObject = projector;
                newHit.OutPointLocal = intersectionChecker.Point + intersectionChecker.LocalDir * 0.2f;
                newHit.LocalNormal = Vector3.forward;
                newHit.ConstructInvolved = projector.GetConstructableOrSubConstructable();
                results.AddAndSort(newHit);
            }
        }
    }
    */
}