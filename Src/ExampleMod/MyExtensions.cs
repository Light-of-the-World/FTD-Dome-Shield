using AdvShields.Models;
using BrilliantSkies.Core.UniverseRepresentation;
using BrilliantSkies.Ftd.Constructs.Modules.All.EMP;
using BrilliantSkies.Ftd.DamageLogging;
using BrilliantSkies.Ftd.DamageModels;
using BrilliantSkies.Ftd.Game.Pools;
using BrilliantSkies.Ftd.Missiles;
using BrilliantSkies.Ftd.Missiles.Blueprints;
using BrilliantSkies.Ftd.Missiles.Components;
using BrilliantSkies.Blocks.MissileComponents;
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
                if (item.ShieldData.Type == enumShieldDomeState.Off) continue;

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
                hit.OutPointGlobal = hitPointIn;
                hit.From = BarrierCondition.Unknown;

                //item.PlayShieldHit(hitPointIn);
                //commented out
                results.AddAndSort(hit);
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

    [HarmonyPatch(typeof(MissileImpactAndTriggering), "HandleHits", new Type[] { typeof(Vector3) })]
    internal class MissileImpactAndTriggering_HandleHits_Patch
    {
        private static void Postfix(MissileImpactAndTriggering __instance)
        {
            Missile _missile = Traverse.Create(__instance).Field("_missile").GetValue<Missile>();

            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.ShieldData.Type == enumShieldDomeState.Off) continue;

                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();

                Vector3 Direction = _missile.Velocity * Time.fixedDeltaTime;

                bool hitSomething = elipse.CheckIntersection(_missile.NosePosition, Direction, out Vector3 hitPointIn, out Vector3 hitNormal);
                if (!hitSomething) continue;

                float range = (_missile.NosePosition - hitPointIn).magnitude;
                if (range > Direction.magnitude * 2) continue;

                // =============== Safety Fuse and Shields by Nicholas Zonenberg ================================
                
                // Bool for checking if the missile has a safety fuse
                bool hasSafetyFuse = false;

                // go through the whole missile checking each component
                foreach( MissileComponent missileComponent in _missile.Blueprint.Components)
                {
                    //enum holding all the comonents
                    enumMissileComponentType componentType = missileComponent.componentType;
                    // 91 is the enum for the safety fuse
                    if ( componentType == (enumMissileComponentType)91 )
                    {
                        hasSafetyFuse = true;
                        break;
                    }
                }

                //if there's a safety fuse check that this vehicle launched it
                if (hasSafetyFuse) { 
                    // the construct the missile is hitting. ( This might need to also check sub constructs but I don't even know if you can put a shield on a sub construct and if you can I have no idea why you would)
                    IMainConstructBlock constructable = item.GetConstructable();
                    //Missile missile = .Missile;
                   // obj that the shield belongs too
                    object obj;
                    if (_missile == null)
                    {
                        obj = null;
                    }
                    else
                    {
                        // get the missilenode ( missile launcher ) the missile came from
                        MissileNode missileNode = _missile.MissileNode;
                        // check if the missile node is on this construct
                        obj = ((missileNode != null) ? missileNode.MainConstruct : null);
                    }
                    // if the missile launcher is on the construct do not hit the shield
                    if (constructable == obj) continue;
                }

                // =============== End Safety Fuses and Shields

                item.ShieldHandler.GridcastHit = hitPointIn;

                IAllConstructBlock allConstructBlock = item.GetConstructableOrSubConstructable();
                Vector3 hitPointInLocal = allConstructBlock.SafeGlobalToLocal(hitPointIn);

                float speed = (_missile.Velocity - allConstructBlock.Velocity).magnitude;
                float damage = _missile.Blueprint.Warheads.ThumpPerMs * speed * _missile.GetHealthDependency(HealthDependency.Damage);
                float thumpAP = _missile.Blueprint.Warheads.ThumpAP;


                item.ShieldHandler.ApplyDamage(new KineticDamageDescription(_missile.Gunner, damage, thumpAP, true));

                bool safetyOn = !__instance.CheckSafety(item) || !_missile.HasClearedVehicle;
                _missile.MoveNoseIntoPosition(hitPointIn);
                _missile.ExplosionHandler.ExplodeNow(allConstructBlock, hitPointInLocal, hitPointIn, safetyOn);
            }
        }
    }

    /* [HarmonyPatch(typeof(EmpDamageDescription), "CalculateEmpDamage", new Type[] { typeof(Vector3) })]
     internal class EmpDamageDescription_CalculateEmpDamage_Patch
     { private static void PostFix(EmpDamageDescription __instance)
         {
             foreach (AdvShieldProjector item in TypeStorage.GetObjects())
             {
                 if (item.ShieldData.Type == enumShieldDomeState.On)
                 { empSusceptibility = 1;
                     DamageFactor = 4;
                 }

             } */

   /* [HarmonyPatch(typeof(ParticleCannonEffect), "ApplyDamage", new Type[] { typeof(Vector3) })]
    internal class ParticleCannonEffect_ApplyDamage_Patch
    {
        private static void Postfix(ParticleCannonEffect __instance);
        {

        
            
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.ShieldData.Type == enumShieldDomeState.Off) continue;

                Elipse elipse = item.ShieldHandler.Shape;
        elipse.UpdateInfo();
                
            if (elipse.CheckIntersection(position, radius))*/
            
            
           
    }





    //[HarmonyPatch] (typeof(ConstructableEmp), "End", new Type[] { typeof Vector3) })]
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