namespace AdvShields
{
    public class AdvShieldDomeData
    {
        private AdvShieldProjector controller;

        public float Energy { get; private set; }

        public float ArmorClass { get; private set; }

        public float SurfaceFactor { get; private set; }

        public float MaxHealth { get; private set; }

        public AdvShieldDomeData(AdvShieldProjector controller)
        {
            this.controller = controller;
            Update();
        }

        public void Update()
        {
            Energy = 0;
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

                float energyForLaser = laserNode.GetTotalEnergyAvailable();

                Energy = controller.ConnectLaserNode.GetMaximumEnergy();
                ArmorClass += ap * 0.5f * (energyForLaser / Energy);
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
    }
}
