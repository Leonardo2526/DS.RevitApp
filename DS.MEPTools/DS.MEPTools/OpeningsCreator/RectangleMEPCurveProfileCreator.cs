using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;

namespace DS.MEPTools.OpeningsCreator
{
    /// <inheritdoc/>
    public class RectangleMEPCurveProfileCreator : RectangleProfileCreatorBase<MEPCurve>
    {
        /// <inheritdoc/>
        public RectangleMEPCurveProfileCreator(Document doc) : base(doc)
        {
        }

        /// <inheritdoc/>
        protected override Solid GetIntersectionSolid(Wall wall, MEPCurve mEPCurve)
            => (wall, mEPCurve).GetIntersectionSolidWithInsulation(0, _activeDoc);
    }
}
