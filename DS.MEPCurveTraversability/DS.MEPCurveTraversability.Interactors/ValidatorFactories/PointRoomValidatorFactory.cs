using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using OLMP.RevitAPI.Tools.Geometry.Solids;
using OLMP.RevitAPI.Tools.Intersections;
using OLMP.RevitAPI.Tools.Rooms.Traversability;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using OLMP.RevitAPI.Tools.Geometry;

namespace DS.MEPCurveTraversability.Interactors.ValidatorFactories
{
    public class PointRoomValidatorFactory : IValidatorFactory
    {
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly IDocumentFilter _globalFilter;
        private readonly IDocumentFilter _localFilter;
        private readonly ITIntersectionFactory<Element, XYZ> _pointIntersectionFactory;
        private readonly Document _doc;

        public PointRoomValidatorFactory(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> allLoadedlinks,
            IDocumentFilter globalFilter,
            IDocumentFilter localFilter,
            ITIntersectionFactory<Element, XYZ> pointIntersectionFactory
            )
        {
            _doc = uiDoc.Document;
            _links = allLoadedlinks;
            _globalFilter = globalFilter;
            _localFilter = localFilter;
            _pointIntersectionFactory = pointIntersectionFactory;
        }

        #region Properties     

    
        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// A factory to commit transactions.
        /// </summary>
        public ITransactionFactory TransactionFactory { get; set; }

        /// <summary>
        /// Window messenger to show important information to user.
        /// </summary>
        public IWindowMessenger WindowMessenger { get; set; }

        /// <summary>
        /// Fields to exclude from <see cref="Room"/>s.
        /// </summary>
        public IEnumerable<string> ExcludeFields { get; set; }

        /// <summary>
        /// Specifies if room names should conatain content fields fully or not.
        /// </summary>
        public bool StrictFieldCompliance { get; set; }

        #endregion


        /// <inheritdoc/>
        public IValidator GetValidator()
        {
            //get rooms
            var roomDocFilter = _localFilter.Clone();
            roomDocFilter.SlowFilters ??= new();
            roomDocFilter.SlowFilters.Add((new RoomFilter(), null));
            var rooms = roomDocFilter.ApplyToAllDocs().
                SelectMany(kv => kv.Value.ToElements(kv.Key)).OfType<Room>();
          
            return new PointRoomValidator(_doc, _links, rooms)
            {
                Logger = Logger,
                ExcludeFields = ExcludeFields,
                WindowMessenger = WindowMessenger,
                PointIntersectionFactory = _pointIntersectionFactory,
                StrictFieldCompliance = StrictFieldCompliance
            };
        }
    }
}
