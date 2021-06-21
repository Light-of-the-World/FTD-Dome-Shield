﻿using System;

namespace AdvShields
{
    public class AdvShieldStatus
    {
        private AdvShieldProjector controller;

        public float Energy { get; private set; }

        public float MaxEnergy { get; private set; }

        public float ArmorClass { get; private set; }

        public float Fragility { get; private set; }

        public AdvShieldStatus(AdvShieldProjector controller)
        {
            this.controller = controller;
            Update();
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

                foreach (LaserCoupler laserCoupler in laserNode.couplers)
                {
                    foreach (BeamInfo beamInfo in laserCoupler.beamInfo)
                    {
                        doublers += beamInfo.FrequencyDoublers;
                        pumps += beamInfo.CubicMetresOfPumping;
                    }

                    int allQSwitches = laserCoupler.NbQSwitches;

                    if (allQSwitches == 0)
                    {
                        Fragility += 40;
                    }
                    else
                    {
                        Fragility += allQSwitches * 2;
                    }
                }

                float surfaceFactor = controller.SurfaceFactor;
                float ap = LaserConstants.GetAp(doublers, pumps, true);

                MaxEnergy = laserNode.GetMaximumEnergy() / surfaceFactor;
                Energy = laserNode.GetTotalEnergyAvailable() / surfaceFactor;
                ArmorClass = ap * 0.5f * (Energy / Math.Max(MaxEnergy, 1f));
            }

            /*
            public float GetCurrentHealth(float sustainedUnfactoredDamage) => (Energy - sustainedUnfactoredDamage) / SurfaceFactor;
            public float GetFactoredDamage(float unfactoredDamage) => unfactoredDamage / 2 * SurfaceFactor;
            */
        }
    }
}
