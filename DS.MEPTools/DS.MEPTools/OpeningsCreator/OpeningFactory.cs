using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Openings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.MEPTools.OpeningsCreator
{
    internal class OpeningFactory<T>
    {
        private readonly IOpeningBuilder<T> _openingBuilder;

        public OpeningFactory(IOpeningBuilder<T> openingBuilder)
        {
            _openingBuilder = openingBuilder;
        }

        public T CreateOpening(Wall wall, MEPCurve mEPCurve)
        {
            var profile = _openingBuilder.CreateProfile(wall, mEPCurve);
            return _openingBuilder.TryExtrude(profile);
        }
    }
}
