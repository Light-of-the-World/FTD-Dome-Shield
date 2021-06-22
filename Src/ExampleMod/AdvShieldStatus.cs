﻿using System;

namespace AdvShields
{
    public class AdvShieldStatus
    {
        private AdvShieldProjector controller;
        private AdvShieldHandler advShieldHandler;

        public float Energy { get; private set; }

        public float MaxEnergy { get; private set; }

        public float ArmorClass { get; private set; }
        
        public float Fragility { get; private set; }

        public AdvShieldStatus(AdvShieldProjector controller)
        {
            this.controller = controller;
            Update();
        }

        public AdvShieldStatus(AdvShieldHandler advShieldHandler)
        {
            this.advShieldHandler = advShieldHandler;
        }

        public void Update()
        {
            Energy = 0;
            ArmorClass = 1;
            Fragility = 0;

            LaserNode laserNode = controller.ConnectLaserNode;

            if (laserNode != null)
            {
                int doublers = 0;
                int pumps = 0;
                int allQSwitches = 0;
                foreach (LaserCoupler laserCoupler in laserNode.couplers)
                {
                    allQSwitches = laserCoupler.NbQSwitches;
                    foreach (BeamInfo beamInfo in laserCoupler.beamInfo)
                    {
                        doublers += beamInfo.FrequencyDoublers;
                        pumps += beamInfo.CubicMetresOfPumping;
                    }

                    if (allQSwitches == 0)
                    {
                        Fragility = 40;
                    }
                    else
                    {
                        Fragility = (allQSwitches * allQSwitches)*1.7f;
                    }
                }

                float surfaceFactor = controller.SurfaceFactor;
                float ap = LaserConstants.GetAp(doublers, pumps, true);
                MaxEnergy = laserNode.GetMaximumEnergy();
                Energy = laserNode.GetTotalEnergyAvailable() / surfaceFactor;
                ArmorClass = ap * 0.5f / (Fragility/2) * (Energy / MaxEnergy);

            }

            /*
            public float GetCurrentHealth(float sustainedUnfactoredDamage) => (Energy - sustainedUnfactoredDamage) / SurfaceFactor;
            public float GetFactoredDamage(float unfactoredDamage) => unfactoredDamage / 2 * SurfaceFactor;
            */
        }
    }
}
