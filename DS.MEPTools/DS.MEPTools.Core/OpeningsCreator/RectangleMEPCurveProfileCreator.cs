using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;

namespace DS.MEPTools.OpeningsCreator
{
    /// <inheritdoc/>
    public class RectangleMEPCurveProfileCreator(Document doc) : RectangleProfileCreatorBase<MEPCurve>(doc)
    {
        /// <inheritdoc/>
        protected override Solid GetIntersectionSolid(Wall wall, MEPCurve mepCurve) =>
            (wall, mepCurve).GetIntersectionSolidWithInsulation(0, ActiveDoc);
    }
}
