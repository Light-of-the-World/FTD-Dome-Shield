using System;

namespace AdvShields
{
    public class AdvShieldStatus
    {
        private AdvShieldProjector controller;

        public float SurfaceFactor { get; private set; } = 1; //surface / AdvShieldDome.BaseSurface;

        public float CurrentEnergy { get; private set; }

        public float MaxEnergy { get; private set; }

        public float ArmorClass { get; private set; }

        public AdvShieldStatus(AdvShieldProjector controller)
        {
            this.controller = controller;
            Update();
        }

        public void Update()
        {
            CurrentEnergy = 0;
            MaxEnergy = 0;
            ArmorClass = 1;

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
                }

                float ap = LaserConstants.GetAp(doublers, pumps, true);

                CurrentEnergy = laserNode.GetTotalEnergyAvailable() / SurfaceFactor;
                MaxEnergy = laserNode.GetMaximumEnergy() / SurfaceFactor;
                ArmorClass = ap * 0.5f * (CurrentEnergy / MaxEnergy);
            }
        }
    }
}
