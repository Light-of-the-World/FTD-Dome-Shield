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
        // Original
        public ShieldDomeBehaviour ShieldDome;
        public ICarriedObjectReference CarriedObject;
        private readonly Rect m_ColoringShieldWindowRect = new Rect(850f, 530f, 425f, 265f);
        public const float BasePowerCost = 0.005f;
        public VelocityMeasurement VelocityMeasurement;
        private BlockModule_Hot Hot;
        public float reliabilityTimeCheck;
        public IPowerRequestRecurring PowerUse;
        private const float ENHANCED_WINDOW_TOP = 530f;
        private const float ENHANCED_WINDOW_LEFT = 850f;
        private const float ENHANCED_WINDOW_MARGIN = 5f;
        private ActivateCallback _activateCallback;

        private Material _material;

        //private static Mesh ShieldMesh;
        //^deleted comment
        public AdvShieldData ShieldData { get; set; } = new AdvShieldData(0U);
        public AdvShieldHandler ShieldHandler { get; set; }
        public AdvShieldVisualData VisualData { get; set; }


        // Laser Properties
        private int _laserSetComponentId = -1;

        private LaserNode[] _laserNodes = new LaserNode[6];

        public Vector3i[] LaserComponentPositions { get; protected set; }
        public IHasLaser[] LaserComponentPromisedTo { get; protected set; }

        public LaserDamageHelper _damageHelper { get; protected set; }

        protected override void RunControl(StimulusDirection stimDirection)
        {
            if (stimDirection == StimulusDirection.Positive)
            {
                ShieldData.ExcessDrive.Us = Mathf.Clamp(ShieldData.ExcessDrive + 2f * UnityEngine.Time.timeScale, 0.0f, 10f);
            }
            else
            {
                if (stimDirection != StimulusDirection.Negative)
                    return;
                ShieldData.ExcessDrive.Us = Mathf.Clamp(ShieldData.ExcessDrive - 2f * UnityEngine.Time.timeScale, 0.0f, 10f);
            }
        }
        public VarIntClamp Priority { get; set; } = new VarIntClamp(0, -50, 50, NoLimitMode.None);
        public void Set(int value)
        {
            this.Priority.Us = value;
        }
        public int Get()
        {
            return this.Priority.Us;
        }
        public PowerUserData PriorityData { get; set; } = new PowerUserData(34852U);
        protected override void RunControlFromDrive(StimulusDirection stimDirection, float driveValue)
        {
            if (stimDirection != StimulusDirection.Positive && stimDirection != StimulusDirection.Negative)
                return;
            driveValue = Mathf.Clamp(driveValue, 0.0f, 10f);
            ShieldData.ExcessDrive.Us = driveValue;
        }

        public override void BlockStart()
        {
            Debug.Log("Advanced Shields: Block Start start");
            base.BlockStart();

            GameObject gameObject = GameObject.Instantiate<GameObject>(StaticStorage.ShieldDomeObject);
            gameObject.transform.position = GameWorldPosition;
            gameObject.transform.rotation = GameWorldRotation;
            gameObject.transform.localPosition = Transforms.LocalToGlobal(Vector3.zero, GameWorldPosition, GameWorldRotation);
            gameObject.transform.localRotation = Transforms.LocalRotationToGlobalRotation(Quaternion.identity, GameWorldRotation);

            CarriedObject = CarryThisWithUs(gameObject, LevelOfDetail.Low);
            ShieldDome = gameObject.GetComponent<ShieldDomeBehaviour>();
            ShieldHandler = new AdvShieldHandler(this);
            //Added Get and Set priority
            //SetShieldSizeAndPosition();
            //PoweredDecoy CurrentPower = base.TargetPower.PowerUse.Us;
            PowerUse = new PowerRequestRecurring(this, PowerRequestType.Shield, new Func<int>
                (this.PriorityData.Get), new Action<int>(this.PriorityData.Set))
            {
                fnSetRequestLevel = new Action<IPowerRequestRecurring>(Allow),
                fnCalculateIdealPowerUse = new Action<IPowerRequestRecurring>(IdealUse),
            };
            // yay
            VelocityMeasurement = new VelocityMeasurement(new UniversePositionReturnBlockInMainFrame(this, PositionReturnBlockValidRequirement.Alive));
            BlockModule_Hot blockModuleHot = new BlockModule_Hot((Block)this);
            blockModuleHot.TemperatureIncreaseUnderFullUsagePerSecond = 0.0f;
            blockModuleHot.CoolingFractionPerSecond = 0.15f;
            blockModuleHot.TotalTemperatureWeighting = 0.2f;
            Hot = blockModuleHot;
            Hot.SetWeightings(LocalForward);

            ShieldData.SetChangeAction(new Action(SetShieldSizeAndPosition), false);

            _material = gameObject.GetComponent<MeshRenderer>().material;
            VisualData = new AdvShieldVisualData(1);
            SetVisualDataEvents();

            ShieldDome.Initialize(_material);

            _activateCallback = new ActivateCallback(this);

            // Laser stuff
            var p = LocalPosition;
            var fw = LocalForward;
            var r = LocalRight;
            var up = LocalUp;

            LaserComponentPromisedTo = new IHasLaser[6];
            LaserComponentPositions = new Vector3i[6]
            {
                p + 2 * fw,
                p - 2 * fw,
                p + 2 * r,
                p - 2 * r,
                p + 2 * up,
                p - 2 * up
            };

            Debug.Log("Advanced Shields: Block Start end");
        }

        protected void AfterLoad()
        {
            base.AfterLoad();
            ConnectToAllLaserSources();
        }

        public override void PrepForDelete()
        {
            //ShieldClass.CleanUp();
        }

        private void SetShieldSizeAndPosition()
        {
            //ShieldClass.SetPositionSizeRotation(new Vector3(0.0f, 0.0f, 0), new Vector3(ShieldData.Width, ShieldData.Height, ShieldData.Length), Quaternion.Euler(ShieldData.Elevation, ShieldData.Azimuth, 0.0f));
            //ShieldClass.SetColor(ShieldData.Color);
            ShieldDome.UpdateSizeInfo(ShieldData);
            VisualData.BaseColor.Us = ShieldData.Color;
            MainConstruct.ShieldsChanged();
        }

        private void SetVisualDataEvents()
        {
            VisualData.AssembleSpeed.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_AssembleSpeed", newValue));

            VisualData.Edge.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_Edge", newValue));
            VisualData.Fresnel.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_Fresnel", newValue));

            VisualData.SinWaveFactor.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_SinWaveFactor", newValue));
            VisualData.SinWaveSpeed.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_SinWaveSpeed", newValue));
            VisualData.SinWaveSize.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_SinWaveSize", newValue));

            VisualData.BaseColor.SetChangeAction((newValue, oldValue, type) => _material.SetColor("_Color", newValue));
            VisualData.GridColor.SetChangeAction((newValue, oldValue, type) => _material.SetColor("_GridColor", newValue));
        }
        //start of deleting comment
        // public bool Reflect(Angle angleOfIncidence, float piercingRoll)
        // {
        //    float magnitude = ProbabilityOfReflection(VelocityMeasurement.Velocity.magnitude, CurrentStrength, angleOfIncidence.FDeg);
        //     bool flag = piercingRoll < magnitude;
        //     if (flag)
        //         ShieldClass.HitByProjectile(magnitude);
        //     else
        //         CriticalCalculator.IsCriticalScored(GetTeam(), CriticalType.ShieldReflect);
        //     return flag;
        // }

        // public static float ProbabilityOfReflection(
        //   float shieldSpeed,
        //   float excessDrive,
        //   float angleOfIncidence)
        //{
        //     float num1 = 0.05f + (float)((double)angleOfIncidence / 90.0 * 0.0500000007450581);
        //     float num2 = (float)(3.0 * ((double)excessDrive - 1.0) / 9.0);
        //     float num3 = Math.Max(0.0f, (float)(1.0 - (double)shieldSpeed / 400.0));
        //     return (float)((double)num1 * (double)num3 * (1.0 + (double)num2));
        // }
        //end of deleting comment
        public override void SetExtraInfo(ExtraInfoArrayReadPackage v)
        {
            base.SetExtraInfo(v);
            v.FindDelimiterAndSpoolToIt(DelimiterType.ShieldProjector, false);
            int orEndOfArrayIfNot = v.ElementsToDelimiterIfThereIsOneOrEndOfArrayIfNot(DelimiterType.ShieldProjector, false);
            if (orEndOfArrayIfNot < 7)
                return;
            ShieldData.Length.Us = v.GetNextFloat();
            ShieldData.Height.Us = v.GetNextFloat();
            ShieldData.Width.Us = v.GetNextFloat();
            ShieldData.ExcessDrive.Us = v.GetNextFloat();
            ShieldData.Azimuth.Us = v.GetNextFloat();
            ShieldData.Elevation.Us = v.GetNextFloat();
            ShieldData.Type.Us = (enumShieldDomeState)v.GetNextInt();
            if (orEndOfArrayIfNot >= 11)
                ShieldData.Color.Us = new Color(v.GetNextFloat(), v.GetNextFloat(), v.GetNextFloat(), v.GetNextFloat());
        }

        public override void SetParameters1(Vector4 v)
        {
            ShieldData.Length.Us = v.x;
            ShieldData.Height.Us = v.y;
            ShieldData.Width.Us = v.z;
            ShieldData.ExcessDrive.Us = v.w;
        }

        public override void SetParameters2(Vector4 v)
        {
            float x = v.x;
            if (x == 0.0)
                ShieldData.Type.Us = enumShieldDomeState.Off;
            else if (x == 1.0)
                ShieldData.Type.Us = enumShieldDomeState.On;
            else if ((double)x == 2.0)
                ShieldData.Type.Us = enumShieldDomeState.On;
            else if ((double)x == 3.0)
                ShieldData.Type.Us = enumShieldDomeState.On;
            ShieldData.Azimuth.Us = v.y;
            ShieldData.Elevation.Us = v.z;
        }

        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);
            float driveAfterFactoring = GetExcessDriveAfterFactoring();
            tip.SetSpecial(UniqueTipType.Name, new ProTipSegment_TitleSubTitle("Shield dome", "Projects a defensive shield around itself"));
            tip.Add(new ProTipSegment_TextAdjustable(500, string.Format("Total drive {0} (basic drive {1} and an external factor of {2})", driveAfterFactoring, ShieldData.ExcessDrive, ShieldData.ExternalDriveFactor)), Position.Middle);
            if (CurrentStrength < (double)driveAfterFactoring)
                tip.Add(new ProTipSegment_TextAdjustable(500, string.Format("Charging, effective drive: {0}", Rounding.R2(CurrentStrength))), Position.Middle);

            switch (ShieldData.Type.Us)
            {
                case enumShieldDomeState.Off:
                    tip.Add(new ProTipSegment_TextAdjustable(500, "This shield turned off and is therefore inactive"), Position.Middle);
                    break;
                case enumShieldDomeState.On:
                    tip.Add(new ProTipSegment_TextAdjustable(500, "This shield is turned on"), Position.Middle);
                    break;
            }

            var domeStats = ShieldHandler.GetDomeStats();
            var currentHealth = domeStats.GetCurrentHealth(ShieldHandler.CurrentDamageSustained);
            tip.Add(new ProTipSegment_Text(400, $"Surface area {(int)ShieldHandler.Shape.SurfaceArea()} m2"), Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has {(int)currentHealth}/{(int)domeStats.MaxHealth} health"), Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has {domeStats.ArmorClass} armor class"), Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has a fragility of {domeStats.SurfaceFactor:F2}"), Position.Middle);

            var text = "Shield is fully charged";
            float progress = 1.0f;

            if (ShieldHandler.CurrentDamageSustained > 0.0f)
            {
                var secondsSinceLastHit = (UnityEngine.Time.time - ShieldHandler.TimeSinceLastHit);
                var timeRemaining = AdvShieldHandler.WaitTime - secondsSinceLastHit;

                if (timeRemaining <= 0.0f)
                {
                    text = $"Shield is recharging, {currentHealth / domeStats.MaxHealth * 100:F1} % complete.";
                }
                else
                {
                    text = $"Time until recharge: {timeRemaining:F1}s";
                    progress = Mathf.Clamp01(Mathf.SmoothStep(0, 1, secondsSinceLastHit / AdvShieldHandler.WaitTime));
                }
            }

            tip.Add(new ProTipSegment_BarWithTextOnIt(400, text, progress));

            tip.Add(new ProTipSegment_TextAdjustable(500, Hot.TemperatureString + ". " + Hot.DirectionString), Position.Middle);
            tip.SetSpecial(UniqueTipType.Interaction, (IProTipSegment)new ProTipSegment_TextAdjustableRight(500, "Press <<Q>> to modify shield settings"));
        }

        public override void Secondary(Transform T)
        {
            new UI.AdvShieldUi(this).ActivateGui(GuiActivateType.Stack);
        }

        public float GetExcessDriveAfterFactoring()
        {
            return Mathf.Clamp((float)ShieldData.ExcessDrive * (float)ShieldData.ExternalDriveFactor, 0.0f, 10f);
        }

        public override void SyncroniseUpdate(bool b1)
        {
            SetShieldState(b1, false);
        }

        [MainThread("Has an RPC and sets an enable flag- must be called from main thread")]
        public void SetShieldState(bool b, bool sync)
        {
            if (!ShieldDome.SetState(b) || !sync || !Net.IsServer)
                return;

            GetConstructableOrSubConstructable().iMultiplayerSyncroniser.RPCRequest_SyncroniseBlock(this, b);
        }

        public bool IsActive
        {
            get
            {
                return ShieldData.Type != enumShieldDomeState.Off;
            }
        }

        public void Allow(IPowerRequestRecurring request)
        {
            VelocityMeasurement.Measure();
            float driveAfterFactoring = GetExcessDriveAfterFactoring();
            RegenerateFromDisruption(request.PowerUsed, request.DeltaTime);
            if (IsOnSubConstructable)
                Hot.SetWeightings(LocalForwardInMainConstruct);
            int num = !IsActive ? 0 : (!DoesConstructHaveOtherShields ? 1 : 0);
            request.InitialRequestLevel = num == 0 ? 0.0f : 1f;
            if (request.InitialRequestLevel == 1.0)
            {
                Hot.TemperatureIncreaseUnderFullUsagePerSecond = (float)(ShieldData.Width * (double)ShieldData.Height * driveAfterFactoring * 0.100000001490116);
                Hot.AddUsage(PowerUse.FractionOfPowerRequestedThatWasProvided);
                ShieldSound.me.NoiseHere(GameWorldPosition, driveAfterFactoring, 1f);
                if (Net.NetworkType == NetworkType.Client || (double)GameTimer.Instance.TimeCache <= reliabilityTimeCheck)
                    return;
                reliabilityTimeCheck = GameTimer.Instance.TimeCache + Aux.RandomRange(0.1f, 1f);
                if (Aux.Rnd.NextFloat(0.0f, 1f) > PowerUse.FractionOfPowerRequestedThatWasProvided)
                    _activateCallback.Enqueue(false, true);
                else
                    _activateCallback.Enqueue(true, true);
            }
            else if (Net.NetworkType != NetworkType.Client)
                _activateCallback.Enqueue(false, true);
        }

        public void IdealUse(IPowerRequestRecurring request)
        {
            if (DoesConstructHaveOtherShields)
                request.IdealPower = 0.0f;
            else if (ShieldData.Type == enumShieldDomeState.Off)
            {
                request.IdealPower = 0.0f;
            }
            else
            {
                float driveAfterFactoring = GetExcessDriveAfterFactoring();
                request.IdealPower = (float)(ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.00499999988824129) * 0.01f;
            }
        }


        public void PlayShieldHit(Vector3 location)
        {
            AudioClipDefinition byCollectionName = Configured.i.AudioCollections.GetRandomClipByCollectionName("Shield Hit");
            if (byCollectionName == null)
                return;
            Pooler.GetPool<AdvSoundManager>().PlaySound(new SoundRequest((IAudioClip)byCollectionName, location)
            {
                Priority = SoundPriority.ShouldHear,
                Pitch = Aux.Rnd.NextFloat(0.9f, 1.1f),
                MinDistance = 0.5f,
                Volume = 0.6f
            });
        }

        public override void CheckStatus(IStatusUpdate updater)
        {
            base.CheckStatus(updater);
            if (ShieldData.Width * ShieldData.Height < 1.00999999046326)
                updater.FlagWarning(this, "Should be larger than 1x1");
            if (!DoesConstructHaveOtherShields)
                return;
            updater.FlagError(this, "Shield domes don't work if there are shield rings or shield projectors on the vehicle");
        }

        public bool DoesConstructHaveOtherShields
        {
            get
            {
                return MainConstruct.NodeSetsRestricted.RingShieldNodes.NodeCount > 0 || MainConstruct.iBlockTypeStorage.ShieldProjectorStore.Count > 0;
            }
        }

        public void Update(ISectorTimeStep fn)
        {
            ConnectToAllLaserSources();
            ShieldHandler.Update();
        }

        public override void StateChanged(IBlockStateChange change)
        {
            base.StateChanged(change);
            if (change.IsAvailableToConstruct)
            {
                //  Objects.Instance.Shields.Add(this)
                TypeStorage.AddObject(this);
                //  MainConstruct.iBlockTypeStorage.ShieldProjectorStore.Add(this); 
                MainConstruct.PowerUsageCreationAndFuelRestricted.AddRecurringPowerUser(PowerUse);
                MainConstruct.HotObjectsRestricted.AddHotObject(Hot);
                MainConstruct.ShieldsChanged();
                //MainConstruct.SchedulerRestricted.RegisterForLateUpdate(new Action(ShieldClass.UpdateColorBasedOnHit));
                MainConstruct.SchedulerRestricted.RegisterFor1PerSecond(new Action<ISectorTimeStep>(Update));
            }
            // else if (!change.IsPerminentlyRemovedfromDesign && !change.IsPerminentlyRemovedOrConstructDestroyed)
            //   ;
            if (!change.IsLostToConstructOrConstructLost)
                return;

            TypeStorage.RemoveObject(this);
            //Objects.Instance.Shields.Remove(base);
            //MainConstruct.iBlockTypeStorage.ShieldProjectorStore.Remove(this);
            MainConstruct.PowerUsageCreationAndFuelRestricted.RemoveRecurringPowerUser(PowerUse);
            MainConstruct.HotObjectsRestricted.RemoveHotObject((IHotObject)Hot);
            MainConstruct.ShieldsChanged();
            //MainConstruct.SchedulerRestricted.UnregisterForLateUpdate(new Action(ShieldClass.UpdateColorBasedOnHit));
            MainConstruct.SchedulerRestricted.UnregisterFor1PerSecond(new Action<ISectorTimeStep>(Update));
        }

        public float CurrentStrength { get; private set; }
        //public object ShieldClass { get; set; }
        // ^added comment
        public override BlockTechInfo GetTechInfo()
        {
            return new BlockTechInfo().AddStatement("Shields have a reduction in reflect effectiveness when moving at high speeds").AddStatement("Shield Domes cannot run when Shield Rings or Shield Projectors are present on your vehicle");
        }

        public void ApplyDisruption(float multiplier)
        {
            CurrentStrength *= multiplier;
        }

        protected void RegenerateFromDisruption(float powerUsed, float deltaTime)
        {
            CurrentStrength = Mathf.Min(CurrentStrength + AdvShieldProjector.GetDisruptionRegenerationRate(powerUsed) * deltaTime, GetExcessDriveAfterFactoring());
        }

        public static float GetDisruptionRegenerationRate(float powerUse)
        {
            return Mathf.Pow(powerUse, 0.5f) * 0.04f;
        }

        public class ActivateCallback : CallbackWithObjects<AdvShieldProjector, bool, bool>
        {
            public ActivateCallback(AdvShieldProjector obj)
              : base(obj)
            {
            }

            protected override void ApplyTo(AdvShieldProjector obj, bool toApply, bool sync)
            {
                obj.SetShieldState(toApply, sync);
            }
        }

        // Laser Stuff
        public override void ItemSet()
        {
            base.ItemSet();
            if (Configured.i == null)
                return;
            bool found;
            ItemGroupDefinition itemGroupDefinition = Configured.i.ItemGroups.Find(new Guid("5166eac9-7c5c-4f6e-a96f-4604d7efbf23"), out found);
            if (!found)
                Debug.Log("Could not find the laser component ID");
            else
                _laserSetComponentId = itemGroupDefinition.ComponentId.RuntimeId;
        }

        public void ConnectToAllLaserSources()
        {
            if (_laserSetComponentId == -1)
                return;

            //Debug.Log("Advanced Shields: Connecting to lasers");
            for (int i = 0; i < LaserComponentPositions.Length; i++)
            {
                if (LaserComponentPromisedTo[i] == null || !LaserComponentPromisedTo[i].IsAlive)
                    LaserComponentPromisedTo[i] = ConnectToALaserSource(LaserComponentPositions[i]);

            }
        }

        public IHasLaser ConnectToALaserSource(Vector3i connectPosition)
        {
            //Debug.Log($"Advanced Shields: Connecting to laser at point {connectPosition}");
            IHasLaser laserPromisedTo = null;

            IAllConstructBlock subConstructable = GetConstructableOrSubConstructable();
            Block viaLocalPosition1 = subConstructable.AllBasicsRestricted.GetAliveBlockInItemGroupViaLocalPosition(connectPosition, _laserSetComponentId);

            if (viaLocalPosition1 != null)
            {
                LaserComponent laserComponent = viaLocalPosition1 as LaserComponent;
                if (laserComponent != null && laserComponent.Node != null)
                    laserPromisedTo = laserComponent;
                LaserMultipurpose laserMultipurpose = viaLocalPosition1 as LaserMultipurpose;
                if (laserMultipurpose != null)
                    laserPromisedTo = laserMultipurpose;
            }

            if (laserPromisedTo?.Node == null)
                return null;

            laserPromisedTo.Node.IsOffensive = true;

            return laserPromisedTo;
        }

        public IEnumerable<LaserNode> LaserPromisedTo()
        {
            if (LaserComponentPromisedTo == null)
            {
                for (int i = 0; i < 6; i++)
                {
                    yield return null;
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    yield return LaserComponentPromisedTo[i]?.Node;
                }
            }
        }

    }
}