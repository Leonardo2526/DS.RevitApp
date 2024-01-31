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

namespace DS.MEPCurveTraversability.Interactors.ValidatorFactories
{
    public class PointRoomValidatorFactory : IValidatorFactory
    {
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly IElementMultiFilter _globalFilter;
        private readonly IElementMultiFilter _localFilter;
        private readonly Document _doc;

        public PointRoomValidatorFactory(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> allLoadedlinks,
            IElementMultiFilter globalFilter,
            IElementMultiFilter localFilter
            )
        {
            _doc = uiDoc.Document;
            _links = allLoadedlinks;
            _globalFilter = globalFilter;
            _localFilter = localFilter;
        }

        #region Properties

        /// <summary>
        /// Ids to exclude from intersections.
        /// </summary>
        public List<ElementId> ExcludedElementIds { get; set; }

        /// <summary>
        /// Types to exclude from intersections.
        /// </summary>
        public List<Type> ExculdedTypes { get; set; }

        /// <summary>
        /// Ids to exclude from intersections.
        /// </summary>
        public List<BuiltInCategory> ExcludedCategories { get; set; }

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
            _localFilter.Reset();
            _localFilter.SlowFilters.Add((new RoomFilter(), null));
            var rooms = _localFilter.ApplyToAllDocs().
                SelectMany(kv => kv.Value.ToElements(kv.Key)).OfType<Room>();

            var pointIntersectionFactory =
                new ElementPointIntersectionFactory(_doc, _links, _globalFilter)
                {
                    Logger = Logger
                };

            if (ExcludedElementIds != null)
            { pointIntersectionFactory.SetExcludedElementIds(ExcludedElementIds); }
            if (ExcludedCategories != null)
            { pointIntersectionFactory.SetExcludedCategories(ExcludedCategories); }
            if (ExculdedTypes != null)
            { pointIntersectionFactory.SetExcludedTypes(ExculdedTypes); }

            return new PointRoomValidator(_doc, _links, rooms)
            {
                Logger = Logger,
                ExcludeFields = ExcludeFields,
                WindowMessenger = WindowMessenger,
                PointIntersectionFactory = pointIntersectionFactory,
                StrictFieldCompliance = StrictFieldCompliance
            };
        }
    }
}
