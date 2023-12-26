using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Openings;

namespace DS.MEPTools.OpeningsCreator
{
    internal class OpeningFactory<T, TIntersectItem>(IOpeningBuilder<T, TIntersectItem> openingBuilder)
    {
        public T CreateOpening(Wall wall, TIntersectItem intersectItem)
        {
            var profile = openingBuilder.CreateProfile(wall, intersectItem);
            return openingBuilder.TryExtrude(profile);
        }
    }
}
