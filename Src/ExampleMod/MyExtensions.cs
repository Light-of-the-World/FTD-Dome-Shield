using AdvShields.Models;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Core.UniverseRepresentation;
using BrilliantSkies.Ftd.DamageLogging;
using BrilliantSkies.Ftd.DamageModels;
using BrilliantSkies.Ftd.Game.Pools;
using BrilliantSkies.GridCasts;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace AdvShields
{
    public static class CIL_Control
    {
        public static int Search(List<CodeInstruction> codes, List<string> searchList)
        {
            int maxCount = codes.Count - searchList.Count;
            int targetIndex = -1;

            for (int i = 0; i < maxCount; i++)
            {
                int count = 0;

                foreach (string str in searchList)
                {
                    if (codes[i + count].ToString() == str)
                    {
                        ++count;
                    }
                    else
                    {
                        count = 0;
                        break;
                    }
                }

                if (count > 0)
                {
                    targetIndex = i;
                    break;
                }
            }

            return targetIndex;
        }
    }

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

    [HarmonyPatch(typeof(ProjectileCastingSystem), "Cast", new Type[] { typeof(ProjectileImpactState), typeof(ISettablePositionAndRotation), typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(float), typeof(Vector3), typeof(int), typeof(Color) })]
    internal class ProjectileCastingSystem_Cast_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<string> searchList = new List<string>()
            {
                "call static ActivePlayAreaCalculator ActivePlayAreaCalculator::get_Instance()",
                "ldarg.s 4",
                "ldarg.s 5",
                "ldarg.s 6",
                "ldc.r4 4",
            };

            List<CodeInstruction> codes = instructions.ToList();
            int targetIndex = CIL_Control.Search(codes, searchList);

            if (targetIndex != -1)
            {
                codes[targetIndex + 4] = new CodeInstruction(OpCodes.Ldc_R4, 1500f);
            }

            return codes.AsEnumerable();
        }
    }

    /*
    [HarmonyPatch(typeof(Fortress), "FixedUpdate", new Type[] { typeof(ITimeStep) })]
    internal class Fortress_FixedUpdate_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<string> searchList = new List<string>()
            {
                "ldarg.0 NULL",
                "call virtual BrilliantSkies.Ftd.Constructs.Modules.All.PartPhysics.IPartPhysics AllConstruct::get_PartPhysics()",
                "callvirt abstract virtual BrilliantSkies.Common.Masses.ConstructableMass BrilliantSkies.Ftd.Constructs.Modules.All.PartPhysics.IPartPhysics::get_TotalMass()",
                "callvirt virtual UnityEngine.Vector3 BrilliantSkies.Common.Masses.ConstructableMass::get_GCOM()",
                "ldfld System.Single UnityEngine.Vector3::y"
            };

            List<CodeInstruction> codes = instructions.ToList();
            int targetIndex = CIL_Control.Search(codes, searchList);

            if (targetIndex != -1)
            {
                codes[targetIndex + 0] = new CodeInstruction(OpCodes.Nop);
                codes[targetIndex + 1] = new CodeInstruction(OpCodes.Nop);
                codes[targetIndex + 2] = new CodeInstruction(OpCodes.Nop);
                codes[targetIndex + 3] = new CodeInstruction(OpCodes.Nop);
                codes[targetIndex + 4] = new CodeInstruction(OpCodes.Ldc_R4, 0f);
            }

            return codes.AsEnumerable();
        }
    }
    */

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