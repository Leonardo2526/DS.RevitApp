using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Revit.MEPAutoCoordination.Offset
{

   public class CollisionChecker
    {
        public bool CheckCollisionType(ICollisionType collisionType)
        {
            return collisionType.Check();
        }
    }
}
