using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;

namespace DS.MEPTools.OpeningsCreator
{
    /// <inheritdoc/>
    public class RectangleSolidProfileCreator : RectangleProfileCreatorBase<Solid>
    {

        /// <inheritdoc/>
        public RectangleSolidProfileCreator(Document doc) : base(doc)
        {
        }

        /// <inheritdoc/>
        protected override Solid GetIntersectionSolid(Wall wall, Solid solid)
        => CollisionUtils.GetIntersectionSolid(ActiveDoc, ActiveDoc, solid, wall.Document, wall.Solid());

    }
}
