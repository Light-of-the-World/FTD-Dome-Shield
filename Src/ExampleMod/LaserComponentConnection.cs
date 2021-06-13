using BrilliantSkies.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvShields
{
    public static class LaserComponentConnection
    {
        public static LaserNode Search(Block block, Vector3i[] verificationPos)
        {
            LaserNode ln = null;

            foreach (Vector3i vp in verificationPos)
            {
                Block b = block.GetConstructableOrSubConstructable().AllBasicsRestricted.GetAliveBlockViaLocalPosition(vp);

                if (b is LaserComponent)
                {
                    LaserComponent lc = b as LaserComponent;
                    ln = lc.Node;

                    break;
                }
            }

            return ln;
        }










    }
}
