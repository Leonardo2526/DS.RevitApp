using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisons;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPTools.Core
{
    /// <summary>
    /// A base class to get intersections of <see cref="Solid"/> with <typeparamref name="T"/> objects.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="elementMultiFilter"></param>
    public abstract class SolidElementIntersectionFactoryBase<T>(
        Document doc,
        IElementMultiFilter elementMultiFilter) :
        ITIntersectionFactory<T, Solid> where T : Element
    {
        /// <summary>
        /// Active <see cref="Document"/>.
        /// </summary>
        protected readonly Document _doc = doc;

        /// <summary>
        /// Elements filtes.
        /// </summary>
        protected IElementMultiFilter _elementMultiFilter = elementMultiFilter;

        /// <summary>
        /// Include only this <see cref="Autodesk.Revit.DB.ElementId"/>s to filter.
        /// </summary>
        public List<ElementId> ElementIdsSet { get; set; }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// A factory to commit transactions.
        /// </summary>
        public ITransactionFactory TransactionFactory { get; set; }

        /// <summary>
        /// Get intersections of <paramref name="solid"/> with <see cref="Autodesk.Revit.DB.Element"/>s.
        /// </summary>
        /// <param name="solid"></param>
        /// <returns>
        /// List of <see cref="Autodesk.Revit.DB.Element"/>s that intersect with <paramref name="solid"/>.
        /// </returns>
        public abstract IEnumerable<T> GetIntersections(Solid solid);

        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.BoundingBoxIntersectsFilter"/> of <paramref name="sourceSolid"/>.
        /// </summary>
        /// <param name="sourceSolid"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.BoundingBoxIntersectsFilter"/> for active <see cref="Autodesk.Revit.DB.Document"/>
        /// and method to get it for <see cref="RevitLinkInstance"/>s.
        /// </returns>
        protected (ElementQuickFilter, Func<Transform, ElementQuickFilter>)
           GetBoundingBoxIntersectsFilter(Solid sourceSolid)
        {
            var solidBox = sourceSolid.GetBoundingBox();
            var boxOutline = solidBox.GetOutline();
            if (TransactionFactory != null) { boxOutline.Show(_doc, TransactionFactory); }
            return (new BoundingBoxIntersectsFilter(boxOutline, 0), trFilter);

            ElementQuickFilter trFilter(Transform linkTransform)
            {
                var linkedSolid = Autodesk.Revit.DB.SolidUtils.CreateTransformed(sourceSolid, linkTransform.Inverse);
                var outlineTransformed = linkedSolid.GetBoundingBox().GetOutline();
                if (TransactionFactory != null) { outlineTransformed.Show(_doc, TransactionFactory); }
                return new BoundingBoxIntersectsFilter(outlineTransformed);
            }
        }
    }
}