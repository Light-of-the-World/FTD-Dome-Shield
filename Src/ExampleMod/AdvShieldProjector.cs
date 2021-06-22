// Decompiled with JetBrains decompiler
// Type: ShieldProjector
// Assembly: Ftd, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BB640B63-E85B-4BC6-BAF1-78BE6814A0C2
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\From The Depths\From_The_Depths_Data\Managed\Ftd.dll

using BrilliantSkies.Common.Controls.AdvStimulii;
using BrilliantSkies.Common.CarriedObjects;
using BrilliantSkies.Common.StatusChecking;
using BrilliantSkies.Core;
using BrilliantSkies.Core.CSharp;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Pooling;
using BrilliantSkies.Core.Returns;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.Returns.UniversePositions;
using BrilliantSkies.Core.Serialisation.AsDouble;
using BrilliantSkies.Core.Threading;
using BrilliantSkies.Core.Threading.Callbacks;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Effects.SoundSystem;
using BrilliantSkies.Effects.SpecialSounds;
using BrilliantSkies.Modding;
using BrilliantSkies.Modding.Types;
using BrilliantSkies.Ui.Displayer;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using UnityEngine;
using AdvShields.Models;
using AdvShields.Behaviours;
using Assets.Scripts;
using BrilliantSkies.Blocks.BlockBaseClass;
using BrilliantSkies.Blocks.Decorative;
using BrilliantSkies.Blocks.Feet;
using BrilliantSkies.Common.ChunkCreators.Chunks.Utilities;
using BrilliantSkies.Common.Colliders;
using BrilliantSkies.Common.Controls;
using BrilliantSkies.Common.Drag;
using BrilliantSkies.Common.Explosions;
using BrilliantSkies.Common.Masses;
using BrilliantSkies.Constructs.Blocks.Parts;
using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.Enumerations;
using BrilliantSkies.Core.Geometry;
using BrilliantSkies.Core.Id;
using BrilliantSkies.Core.Intersections;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Maths;
using BrilliantSkies.Core.Recursion;
using BrilliantSkies.Core.ResourceAccess;
using BrilliantSkies.Core.Returns.Interfaces;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;
using BrilliantSkies.Core.Units;
using BrilliantSkies.Core.Unity.MeshMaking;
using BrilliantSkies.Core.UniverseRepresentation.Positioning.Frames.Points;
using BrilliantSkies.Core.Widgets;
using BrilliantSkies.Effects.Pools.Smashes;
using BrilliantSkies.Effects.Regulation;
using BrilliantSkies.Ftd.Avatar;
using BrilliantSkies.Ftd.Avatar.Repair;
using BrilliantSkies.Ftd.Constructs.Modules.All.Shell;
using BrilliantSkies.Ftd.Constructs.Modules.All.StandardExplosion;
using BrilliantSkies.Ftd.Constructs.Modules.Main.Scuttling;
using BrilliantSkies.Ftd.DamageLogging;
using BrilliantSkies.Ftd.DamageModels;
using BrilliantSkies.Ftd.Game.Pools;
using BrilliantSkies.GridCasts;
using BrilliantSkies.GridCasts.Interfaces;
using BrilliantSkies.Localisation;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Modding.Containers;
using BrilliantSkies.Modding.Types.Helpful;
using BrilliantSkies.Ui.Consoles.Styles;
using BrilliantSkies.Ui.Special.ButtonsAndBars;
using BrilliantSkies.FromTheDepths.Game.UserInterfaces;



namespace AdvShields
{
    public class AdvShieldProjector : BlockWithControl
    {
        private ICarriedObjectReference carriedObject;

        private BlockModule_Hot module_Hot;

        private ActivateCallback activateCallback;

        private VelocityMeasurement velocityMeasurement;

        private float reliabilityTimeCheck;

        private float currentStrength;
        private AdvShieldHandler advShieldHandler;

        public AdvShieldProjector(AdvShieldHandler advShieldHandler)
        {
            this.advShieldHandler = advShieldHandler;
        }

        public virtual float SurfaceFactor { get; private set; } = 1; //surface / AdvShieldDome.BaseSurface;
        
        public float TimeRemaining { get; set; }

        public ShieldDomeBehaviour ShieldDome { get; set; }

        public AdvShieldHandler ShieldHandler { get; set; }

        public AdvShieldStatus ShieldStats { get; set; }

        public AdvShieldData ShieldData { get; set; } = new AdvShieldData(0u);

        public AdvShieldVisualData VisualData { get; set; } = new AdvShieldVisualData(1u);

        public LaserNode ConnectLaserNode { get; set; }

        public IPowerRequestRecurring PowerUse { get; set; }

        public VarIntClamp Priority { get; set; } = new VarIntClamp(0, -50, 50, NoLimitMode.None);

        public PowerUserData PriorityData { get; set; } = new PowerUserData(34852u);

        public bool IsActive
        {
            get
            {
                return ShieldData.Type != enumShieldDomeState.Off;
            }
        }

        public bool DoesConstructHaveOtherShields
        {
            get
            {
                return MainConstruct.NodeSetsRestricted.RingShieldNodes.NodeCount > 0 || MainConstruct.iBlockTypeStorage.ShieldProjectorStore.Count > 0;
            }
        }

        public override void BlockStart()
        {
            base.BlockStart();

            Debug.Log("Advanced Shields: Block Start start");

            GameObject gameObject = GameObject.Instantiate<GameObject>(StaticStorage.ShieldDomeObject);
            gameObject.transform.position = GameWorldPosition;
            gameObject.transform.rotation = GameWorldRotation;
            gameObject.transform.localPosition = Transforms.LocalToGlobal(Vector3.zero, GameWorldPosition, GameWorldRotation);
            gameObject.transform.localRotation = Transforms.LocalRotationToGlobalRotation(Quaternion.identity, GameWorldRotation);

            carriedObject = CarryThisWithUs(gameObject, LevelOfDetail.Low);

            ShieldDome = gameObject.GetComponent<ShieldDomeBehaviour>();
            ShieldDome.Initialize();

            ShieldStats = new AdvShieldStatus(this);
            float num = ShieldStats.Fragility;
            ShieldHandler = new AdvShieldHandler(this);

            // yay
            velocityMeasurement = new VelocityMeasurement(new UniversePositionReturnBlockInMainFrame(this, PositionReturnBlockValidRequirement.Alive));

            //Added Get and Set priority
            //PoweredDecoy CurrentPower = base.TargetPower.PowerUse.Us;
            PowerUse = new PowerRequestRecurring(this, PowerRequestType.Shield, PriorityData.Get, PriorityData.Set)
            {
                fnSetRequestLevel = Allow,
                fnCalculateIdealPowerUse = IdealUse,
            };

            module_Hot = new BlockModule_Hot(this);
            module_Hot.TemperatureIncreaseUnderFullUsagePerSecond = 0.0f;
            module_Hot.CoolingFractionPerSecond = 0.15f;
            module_Hot.TotalTemperatureWeighting = 0.2f;
            module_Hot.SetWeightings(LocalForward);

            activateCallback = new ActivateCallback(this);

            ShieldDataSetChangeAction();
            VisualDataSetChangeAction();

            Debug.Log("Advanced Shields: Block Start end");
        }



        public override void StateChanged(IBlockStateChange change)
        {
            base.StateChanged(change);

            if (change.IsAvailableToConstruct)
            {
                TypeStorage.AddObject(this);
                MainConstruct.PowerUsageCreationAndFuelRestricted.AddRecurringPowerUser(PowerUse);
                MainConstruct.HotObjectsRestricted.AddHotObject(module_Hot);
                MainConstruct.ShieldsChanged();
                MainConstruct.SchedulerRestricted.RegisterForLateUpdate(Update);
            }

            if (change.IsLostToConstructOrConstructLost)
            {
                TypeStorage.RemoveObject(this);
                MainConstruct.PowerUsageCreationAndFuelRestricted.RemoveRecurringPowerUser(PowerUse);
                MainConstruct.HotObjectsRestricted.RemoveHotObject(module_Hot);
                MainConstruct.ShieldsChanged();
                MainConstruct.SchedulerRestricted.UnregisterForLateUpdate(Update);
            }
        }

        public override void CheckStatus(IStatusUpdate updater)
        {
            base.CheckStatus(updater);

            if (ShieldData.Width * ShieldData.Height < 1.00999999046326)
            {
                updater.FlagWarning(this, "Should be larger than 1x1");
            }

            if (DoesConstructHaveOtherShields)
            {
                updater.FlagError(this, "Shield domes don't work if there are shield rings or shield projectors on the vehicle");
            }

            ConnectLaserNode = LaserComponentSearch();
        }

        public override void PrepForDelete()
        {
            //ShieldClass.CleanUp();
        }

        public override void SyncroniseUpdate(bool b1)
        {
            SetShieldState(b1, false);
        }



        protected override void RunControl(StimulusDirection stimDirection)
        {
            if (stimDirection == StimulusDirection.Positive)
            {
                ShieldData.ExcessDrive.Us = Mathf.Clamp(ShieldData.ExcessDrive + 2f * UnityEngine.Time.timeScale, 0.0f, 10f);
            }
            else if (stimDirection == StimulusDirection.Negative)
            {
                ShieldData.ExcessDrive.Us = Mathf.Clamp(ShieldData.ExcessDrive - 2f * UnityEngine.Time.timeScale, 0.0f, 10f);
            }
        }

        protected override void RunControlFromDrive(StimulusDirection stimDirection, float driveValue)
        {
            if (stimDirection == StimulusDirection.None) return;

            driveValue = Mathf.Clamp(driveValue, 0.0f, 10f);
            ShieldData.ExcessDrive.Us = driveValue;
        }



        public override BlockTechInfo GetTechInfo()
        {
            return new BlockTechInfo().AddStatement("Shields have a reduction in reflect effectiveness when moving at high speeds").AddStatement("Shield Domes cannot run when Shield Rings or Shield Projectors are present on your vehicle");
        }

        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);

            float driveAfterFactoring = GetExcessDriveAfterFactoring();
            bool flag_0 = currentStrength < driveAfterFactoring;
            string text_0 = "This shield turned off and is therefore inactive";

            if (ShieldData.Type.Us == enumShieldDomeState.On)
            {
                text_0 = "This shield is turned on";
            }
            float currentHealth = ShieldHandler.GetCurrentHealth();
            string text_1 = "Shield is fully charged";
            float progress = 1.0f;

            if (ShieldHandler.CurrentDamageSustained > 0.0f)
            {
                float secondsSinceLastHit = UnityEngine.Time.time - ShieldHandler.TimeSinceLastHit;
                float TimeRemaining = 45  -(ShieldStats.Fragility/1.1f) -secondsSinceLastHit;

                if (this.TimeRemaining <= 0.0f)
                {
                    text_1 = $"Shield is recharging, {currentHealth / ShieldStats.MaxEnergy * 100:F1} % complete.";
                }
                else
                {
                    text_1 = $"Time until recharge: {this.TimeRemaining:F1}s";
                    progress = Mathf.Clamp01(Mathf.SmoothStep(0, 1, secondsSinceLastHit / AdvShieldHandler.WaitTime));
                }
            }

            tip.SetSpecial(UniqueTipType.Name, new ProTipSegment_TitleSubTitle("Shield dome", "Projects a defensive shield around itself"));
            tip.Add(new ProTipSegment_TextAdjustable(500, string.Format("Total drive {0} (basic drive {1} and an external factor of {2})", driveAfterFactoring, ShieldData.ExcessDrive, ShieldData.ExternalDriveFactor)), Position.Middle);
            if (flag_0) tip.Add(new ProTipSegment_TextAdjustable(500, string.Format("Charging, effective drive: {0}", Rounding.R2(currentStrength))), Position.Middle);
            tip.Add(new ProTipSegment_TextAdjustable(500, text_0), Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"Surface area {(int)ShieldHandler.Shape.SurfaceArea()} m2"), Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has {(int)currentHealth}/{(int)ShieldStats.MaxEnergy} health"), Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has {ShieldStats.ArmorClass} armor class"), Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has a fragility of {ShieldStats.Fragility}"), Position.Middle);
            tip.Add(new ProTipSegment_BarWithTextOnIt(400, text_1, progress));
            tip.Add(new ProTipSegment_TextAdjustable(500, module_Hot.TemperatureString + ". " + module_Hot.DirectionString), Position.Middle);
            tip.SetSpecial(UniqueTipType.Interaction, new ProTipSegment_TextAdjustableRight(500, "Press <<Q>> to modify shield settings"));
        }

        public override void Secondary(Transform T)
        {
            new UI.AdvShieldUi(this).ActivateGui(GuiActivateType.Stack);
        }



        public virtual Vector3i[] SetVerificationPosition()
        {
            Vector3i p = LocalPosition;
            Vector3i fv = LocalForward;
            Vector3i rv = LocalRight;
            Vector3i uv = LocalUp;

            Vector3i[] verificationPosition = new Vector3i[6]
            {
                p + 2 * fv,
                p - 2 * fv,
                p + 2 * rv,
                p - 2 * rv,
                p + 2 * uv,
                p - 2 * uv
            };

            return verificationPosition;
        }



        private void Allow(IPowerRequestRecurring request)
        {
            velocityMeasurement.Measure();
            float driveAfterFactoring = GetExcessDriveAfterFactoring();
            //RegenerateFromDisruption(request.PowerUsed, request.DeltaTime);
            currentStrength = Mathf.Min(currentStrength + ShieldProjector.GetDisruptionRegenerationRate(request.PowerUsed) * request.DeltaTime, driveAfterFactoring);

            if (IsOnSubConstructable)
            {
                module_Hot.SetWeightings(LocalForwardInMainConstruct);
            }

            int num = (IsActive && !DoesConstructHaveOtherShields) ? 1 : 0;
            request.InitialRequestLevel = num;

            if (request.InitialRequestLevel == 1f)
            {
                module_Hot.TemperatureIncreaseUnderFullUsagePerSecond = (float)(ShieldData.Width * (double)ShieldData.Height * driveAfterFactoring * 0.100000001490116);
                module_Hot.AddUsage(PowerUse.FractionOfPowerRequestedThatWasProvided);
                ShieldSound.me.NoiseHere(GameWorldPosition, driveAfterFactoring, 1f);

                if (Net.NetworkType == NetworkType.Client || (double)GameTimer.Instance.TimeCache <= reliabilityTimeCheck)
                {
                    return;
                }

                reliabilityTimeCheck = GameTimer.Instance.TimeCache + Aux.RandomRange(0.1f, 1f);

                bool flag_0 = Aux.Rnd.NextFloat(0.0f, 1f) > PowerUse.FractionOfPowerRequestedThatWasProvided;
                activateCallback.Enqueue(!flag_0, true);
            }
            else if (Net.NetworkType != NetworkType.Client)
            {
                activateCallback.Enqueue(false, true);
            }
        }

        private void IdealUse(IPowerRequestRecurring request)
        {
            if (DoesConstructHaveOtherShields)
            {
                request.IdealPower = 0f;
            }

            else if (ShieldData.Type == enumShieldDomeState.Off)
            {
                request.IdealPower = 0f;
            }
            else
            {
                float driveAfterFactoring = GetExcessDriveAfterFactoring();
                request.IdealPower = (float)(ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.00499999988824129) * 0.01f;
            }
        }



        [MainThread("Has an RPC and sets an enable flag- must be called from main thread")]
        private void SetShieldState(bool b, bool sync)
        {
            if (!ShieldDome.SetState(b) || !sync || !Net.IsServer) return;

            GetConstructableOrSubConstructable().iMultiplayerSyncroniser.RPCRequest_SyncroniseBlock(this, b);
        }

        public float GetExcessDriveAfterFactoring()
        {
            return Mathf.Clamp(ShieldData.ExcessDrive * ShieldData.ExternalDriveFactor, 0f, 10f);
        }

        public void Update()
        {
            ShieldStats.Update();
            ShieldHandler.Update();
        }

        private void ShieldDataSetChangeAction()
        {
            ShieldData.SetChangeAction(
            () =>
            {
                ShieldHandler.Shape.UpdateInfo();
                ShieldDome.UpdateSizeInfo(ShieldData);
                carriedObject.ObjectItself.transform.localPosition = LocalPosition + new Vector3(ShieldData.LocalPosX, ShieldData.LocalPosY, ShieldData.LocalPosZ);
            });
        }

        private void VisualDataSetChangeAction()
        {
            Material _material = carriedObject.ObjectItself.GetComponent<MeshRenderer>().material;

            VisualData.AssembleSpeed.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_AssembleSpeed", newValue));
            VisualData.Edge.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_Edge", newValue));
            VisualData.Fresnel.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_Fresnel", newValue));
            VisualData.SinWaveFactor.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_SinWaveFactor", newValue));
            VisualData.SinWaveSpeed.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_SinWaveSpeed", newValue));
            VisualData.SinWaveSize.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_SinWaveSize", newValue));
            VisualData.BaseColor.SetChangeAction((newValue, oldValue, type) => _material.SetColor("_Color", newValue));
            VisualData.GridColor.SetChangeAction((newValue, oldValue, type) => _material.SetColor("_GridColor", newValue));
        }

        private LaserNode LaserComponentSearch()
        {
            Vector3i[] verificationPosition = SetVerificationPosition();
            LaserNode ln = null;

            foreach (Vector3i vp in verificationPosition)
            {
                Block b = GetConstructableOrSubConstructable().AllBasicsRestricted.GetAliveBlockViaLocalPosition(vp);

                if (b is LaserConnector || b is LaserTransceiver)
                {
                    LaserComponent lc = b as LaserComponent;
                    ln = lc.Node;
                    break;
                }
                else if (b is LaserMultipurpose)
                {
                    LaserMultipurpose lm = b as LaserMultipurpose;
                    ln = lm.Node;
                    break;
                }
            }

            return ln;
        }

        public void PlayShieldHit(Vector3 location)
        {
            AudioClipDefinition byCollectionName = Configured.i.AudioCollections.GetRandomClipByCollectionName("Shield Hit");
            if (byCollectionName == null) return;

            Pooler.GetPool<AdvSoundManager>().PlaySound(new SoundRequest(byCollectionName, location)
            {
                Priority = SoundPriority.ShouldHear,
                Pitch = Aux.Rnd.NextFloat(0.9f, 1.1f),
                MinDistance = 0.5f,
                Volume = 0.6f
            });
        }



        public class ActivateCallback : CallbackWithObjects<AdvShieldProjector, bool, bool>
        {
            public ActivateCallback(AdvShieldProjector obj) : base(obj)
            {
            }

            protected override void ApplyTo(AdvShieldProjector obj, bool toApply, bool sync)
            {
                obj.SetShieldState(toApply, sync);
            }
        }
    }
}