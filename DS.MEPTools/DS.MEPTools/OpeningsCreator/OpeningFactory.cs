using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Openings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.MEPTools.OpeningsCreator
{
    internal class OpeningFactory<T, TIntersectItem>
    {
        private readonly IOpeningBuilder<T, TIntersectItem> _openingBuilder;

        public OpeningFactory(IOpeningBuilder<T, TIntersectItem> openingBuilder)
        {
            _openingBuilder = openingBuilder;
        }

        public T CreateOpening(Wall wall, TIntersectItem intersectItem)
        {
            var profile = _openingBuilder.CreateProfile(wall, intersectItem);
            return _openingBuilder.TryExtrude(profile);
        }
    }
}
