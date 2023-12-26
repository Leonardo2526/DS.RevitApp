using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Openings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.MEPTools.OpeningsCreator
{
    internal class OpeningBuilder : IOpeningBuilder<Solid, MEPCurve>
    {
        public OpeningBuilder()
        {

        }

        public CurveLoop CreateProfile(Wall wall, MEPCurve mEPCurve)
        {
            throw new NotImplementedException();
        }

        public Solid TryExtrude(CurveLoop profile)
        {
            throw new NotImplementedException();
        }
    }
}
