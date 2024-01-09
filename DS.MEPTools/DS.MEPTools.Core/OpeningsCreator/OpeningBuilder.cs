using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Openings;
using System;

namespace DS.MEPTools.Core.OpeningsCreator
{
    internal class OpeningBuilder : IOpeningBuilder<Solid, MEPCurve>
    {
        public CurveLoop CreateProfile(Wall wall, MEPCurve mepCurve)
        {
            throw new NotImplementedException();
        }

        public Solid TryExtrude(CurveLoop profile)
        {
            throw new NotImplementedException();
        }
    }
}
