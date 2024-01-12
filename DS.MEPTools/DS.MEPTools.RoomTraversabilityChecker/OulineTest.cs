using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry;
using Autodesk.Revit.DB.Architecture;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using System.Collections;
using MoreLinq;
using Autodesk.Revit.UI;
using System.Linq;
using System.Diagnostics;
using DS.ClassLib.VarUtils.Basis;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using DS.RevitLib.Utils.Extensions;
using DS.MEPTools.Core.Rooms;

namespace DS.MEPTools.RoomTraversabilityChecker
{
    internal class OulineTest
    {
        private readonly UIDocument _uiDoc;
        private readonly IElementMultiFilter _elementMultiFilter;
        private readonly Document _doc;

        public OulineTest(UIDocument uiDoc, IElementMultiFilter elementMultiFilter)
        {
            _uiDoc = uiDoc;
            _elementMultiFilter = elementMultiFilter;
            _doc = uiDoc.Document;
        }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// A factory to commit transactions.
        /// </summary>
        public ITransactionFactory TransactionFactory { get; set; }

        public void Run(MEPCurve mEPCurve)
        {
            var solid = mEPCurve.Solid();
            //var roomDocs = GetElementDocs(mEPCurve, solid, _elementMultiFilter);
            //var roomBoxDocs = GetElementDocs(mEPCurve, solid, _elementMultiFilter);
            //var rooms = roomBoxDocs.SelectMany(kv => ToRooms(kv.Key, kv.Value));
            //rooms = rooms.Where(r => r.Contains(solid, _doc));
            var rooms = new SolidRoomIntersectionFactory(_doc, _elementMultiFilter)
            { Logger = Logger, TransactionFactory = TransactionFactory }.
                GetIntersections(solid);


            Debug.Flush();
            if (rooms.Count() == 0)
            {
                Logger?.Warning("No intersection elements.");
            }
            rooms.ForEach(r => { Logger?.Information(r.Name); });
            //foreach (var elemDoc in roomsElementsIds)
            //{
            //    var elIds = elemDoc.Value.ToList();
            //    _uiDoc.Selection.SetElementIds(elIds);
            //    elIds.ForEach(id => Logger?.Information(ToRoom(elemDoc.Key, id).Name));
            //}
        }

        private Dictionary<Document, IEnumerable<ElementId>> GetElementDocs(
          MEPCurve mEPCurve, Solid mEPCurveSolid,
          IElementMultiFilter elementMultiFilter)
        {
            //config quick filters
            var mEPCurveBoundingBoxFilter = GetBoundingBoxIntersectsFilter(mEPCurve);
            elementMultiFilter.QuickFilters.Add(mEPCurveBoundingBoxFilter);

            //config slow filters
            elementMultiFilter.SlowFilters.Add((new RoomFilter(), null));
            //var mEPCurveSolidFilter = GetRoomSolidFilter(mEPCurveSolid);
            //elementMultiFilter.SlowFilters.Add(mEPCurveSolidFilter);

            return elementMultiFilter.ApplyToAllDocs();

            (ElementQuickFilter, Func<Transform, ElementQuickFilter>)
                GetBoundingBoxIntersectsFilter(MEPCurve mEPCurve)
            {
                var mEPCurveBox = mEPCurve.get_BoundingBox(null);
                var mEPCurveOutline = mEPCurveBox.GetOutline();
                mEPCurveOutline.Show(_doc, TransactionFactory);
                return (new BoundingBoxIntersectsFilter(mEPCurveOutline, 0), trFilter);

                ElementQuickFilter trFilter(Transform linkTransform)
                {
                    var mEPCurveOutlineTransformed =
                        GetTransformed3(mEPCurve, mEPCurveSolid, linkTransform.Inverse);
                    mEPCurveOutlineTransformed.Show(_doc, TransactionFactory);
                    return new BoundingBoxIntersectsFilter(mEPCurveOutlineTransformed);
                }
            }

            (ElementSlowFilter, Func<Transform, ElementSlowFilter>) 
                GetRoomSolidFilter(Solid mEPCurveSolid)
            {
                return (new ElementIntersectsSolidFilter(mEPCurveSolid), trFilter);

                ElementSlowFilter trFilter(Transform linkTransform)
                {
                    var mEPCurveSolidTransformed = Autodesk.Revit.DB.SolidUtils.CreateTransformed(mEPCurveSolid, linkTransform.Inverse);
                    return new ElementIntersectsSolidFilter(mEPCurveSolidTransformed);
                }
            }
        }

        private Room ToRoom(Document roomDoc, ElementId roomId)
        {
            var elem = roomDoc.GetElement(roomId);
           return elem as Room; 
        }

        private Outline GetTransformed(Outline outline, Transform transform)
        {
            //create rhino Box
            var rMinPoint = outline.MinimumPoint.ToPoint3d();
            var rMaxPoint = outline.MaximumPoint.ToPoint3d();

            var rBox = new Rhino.Geometry.BoundingBox(rMinPoint, rMaxPoint);
            var sourceOriginBasis = new Basis3dOrigin();
            var sourceBasis = new Basis3d(
                transform.Origin.ToPoint3d(),
                sourceOriginBasis.X,
                sourceOriginBasis.Y,
                sourceOriginBasis.Z);
            var targetBasis = new Basis3d(
                transform.Origin.ToPoint3d(),
                transform.BasisX.ToVector3d(),
                transform.BasisY.ToVector3d(),
                transform.BasisZ.ToVector3d());
            //Rhino.Geometry.Transform rRransform = targetBasis.GetTransform(sourceBasis);
            Rhino.Geometry.Transform rRransform = sourceBasis.GetTransform(targetBasis);
            if (!rBox.Transform(rRransform))
            {
                Logger?.Error("Failed to transform.");
            }

            //var minPoint = outline.MinimumPoint;
            //var maxPoint = outline.MaximumPoint;
            var minPoint = rBox.Min.ToXYZ();
            var maxPoint = rBox.Max.ToXYZ();
            var l1 = 200.MMToFeet();
            var l2 = 300.MMToFeet();
            TransactionFactory.CreateAsync(() => outline.MinimumPoint.ShowPoint(_doc, l1), "ShowMinSourcePoint");
            TransactionFactory.CreateAsync(() => outline.MaximumPoint.ShowPoint(_doc, l1), "ShowMaxSourcePoint");
            //TransactionFactory.CreateAsync(() => p1.ShowPoint(_doc, l2), "ShowP1");
            //TransactionFactory.CreateAsync(() => p2.ShowPoint(_doc, l2), "ShowP2");

            TransactionFactory.CreateAsync(() => minPoint.ShowPoint(_doc, l2), "ShowMinPoint");
            TransactionFactory.CreateAsync(() => maxPoint.ShowPoint(_doc, l2), "ShowMaxPoint");

            return new Outline(minPoint, maxPoint);
        }

        private Outline GetTransformed2(Outline outline, Transform transform)
        {
            //create rhino Box
            var rMinPoint = outline.MinimumPoint.ToPoint3d();
            var rMaxPoint = outline.MaximumPoint.ToPoint3d();

            var p1 = transform.OfPoint(outline.MinimumPoint);
            var p2 = transform.OfPoint(outline.MaximumPoint);
            var l1 = 200.MMToFeet();
            var l2 = 300.MMToFeet();
            TransactionFactory.CreateAsync(() => outline.MinimumPoint.ShowPoint(_doc, l1), "ShowMinSourcePoint");
            TransactionFactory.CreateAsync(() => outline.MaximumPoint.ShowPoint(_doc, l1), "ShowMaxSourcePoint");
            //TransactionFactory.CreateAsync(() => p1.ShowPoint(_doc, l2), "ShowP1");
            //TransactionFactory.CreateAsync(() => p2.ShowPoint(_doc, l2), "ShowP2");

            (XYZ minPoint, XYZ maxPoint) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p1, p2 });
            TransactionFactory.CreateAsync(() => minPoint.ShowPoint(_doc, l2), "ShowMinPoint");
            TransactionFactory.CreateAsync(() => maxPoint.ShowPoint(_doc, l2), "ShowMaxPoint");

            return new Outline(minPoint, maxPoint);
        }

        private Outline GetTransformed3(MEPCurve mEPCurve, Solid mEPCurveSolid, Transform transform)
        {
            var mEPCurveLinkSolid = Autodesk.Revit.DB.SolidUtils.CreateTransformed(mEPCurveSolid, transform);
            var transformedBox = mEPCurveLinkSolid.GetBoundingBox();
            var outline = transformedBox.GetOutline();

            var p1 = outline.MinimumPoint;
            var p2 = outline.MaximumPoint;
            var l1 = 200.MMToFeet();
            var l2 = 300.MMToFeet();
            TransactionFactory.CreateAsync(() => outline.MinimumPoint.ShowPoint(_doc, l1), "ShowMinSourcePoint");
            TransactionFactory.CreateAsync(() => outline.MaximumPoint.ShowPoint(_doc, l1), "ShowMaxSourcePoint");
            //TransactionFactory.CreateAsync(() => p1.ShowPoint(_doc, l2), "ShowP1");
            //TransactionFactory.CreateAsync(() => p2.ShowPoint(_doc, l2), "ShowP2");

            (XYZ minPoint, XYZ maxPoint) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p1, p2 });
            TransactionFactory.CreateAsync(() => minPoint.ShowPoint(_doc, l2), "ShowMinPoint");
            TransactionFactory.CreateAsync(() => maxPoint.ShowPoint(_doc, l2), "ShowMaxPoint");

            return new Outline(minPoint, maxPoint);
        }

        private Dictionary<Document, IEnumerable<ElementId>> GetElementsInsideRoooms(
           Dictionary<Document, IEnumerable<ElementId>> roomDocs,
           Document doc)
        {
            var roomsElementsIdsDict = new Dictionary<Document, IEnumerable<ElementId>>();

            foreach (var kv in roomDocs)
            {
                var roomDoc = kv.Key;
                var roomIds = kv.Value;
                var roomsElementsIds = new List<ElementId>();   
                foreach (var roomId in roomIds)
                {
                    var room = roomDoc.GetElement(roomId) as Room;
                    var elementIds = room.GetInsideElements(doc, null).ToList();
                    foreach (var id in elementIds)
                    {
                        if (!roomsElementsIds.Contains(id))
                        { roomsElementsIds.Add(id); }
                    }
                    Logger?.Information($"Elements in room '{room.Name}': {elementIds.Count()}");
                }
            }
            Logger?.Information($"Total rooms elements: {roomsElementsIdsDict.Count()}");

            //_uiDoc.Selection.SetElementIds( roomsElements );

            return roomsElementsIdsDict;
        }

        private IEnumerable<Room> ToRooms(Document roomDoc, IEnumerable<ElementId> roomIds)
        {
            var rooms = new List<Room>();
            roomIds.ToList().ForEach(id => rooms.Add(ToRoom(roomDoc, id)));
            return rooms;
        }
    }
}
