using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Rooms;
using Rhino.UI;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.MEPCurveTraversability.Interactors
{
    public class TraversabilityService
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly IElementMultiFilter _elementMultiFilter;
        private readonly ITIntersectionFactory<Element, Solid> _intersectionFactory;
        private readonly WallIntersectionSettings _intersectionSettings;
        bool checkRooms = true;


        public TraversabilityService(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> links,
            IElementMultiFilter elementMultiFilter,
            ITIntersectionFactory<Element, Solid> intersectionFactory,
            WallIntersectionSettings intersectionSettings)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _links = links;
            _elementMultiFilter = elementMultiFilter;
            _intersectionFactory = intersectionFactory;
            _intersectionSettings = intersectionSettings;
        }

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


        public bool Initiate(MEPCurve mEPCurve)
        {
            if (checkRooms)
            {
                var solidRoomIntersectFactory = new SolidRoomIntersectionFactory(_doc, _links, _elementMultiFilter)
                { Logger = Logger, TransactionFactory = null };
                var roomCheker = new RoomChecker(
                    _uiDoc, _links,
                    _elementMultiFilter,
                    _intersectionFactory,
                    solidRoomIntersectFactory)
                {
                    Logger = Logger,
                    //TransactionFactory = trf,
                    WindowMessenger = WindowMessenger
                }.Initiate(mEPCurve);

                if (!roomCheker) { return false; }
            }

           var wallChecker = new WallsChecker(_uiDoc, _links, _intersectionFactory, _intersectionSettings)
            {
                Logger = Logger,
                //TransactionFactory = trf,
                WindowMessenger = WindowMessenger

            };
            return wallChecker.Initiate(mEPCurve);
        }
    }
}
