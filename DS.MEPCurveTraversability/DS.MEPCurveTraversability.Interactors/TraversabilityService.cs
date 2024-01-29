using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using DS.MEPCurveTraversability.Interactors.Settings;
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
using OLMP.RevitAPI.Core.Extensions;

namespace DS.MEPCurveTraversability.Interactors
{
    public class TraversabilityService
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _allDoclinks;
        private readonly IElementMultiFilter _globalFilter;
        private readonly DocSettingsAR _settingsAR;
        bool checkRooms = true;


        public TraversabilityService(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> allDocLinks,
            IElementMultiFilter elementMultiFilter,
            DocSettingsAR settingsAR)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _allDoclinks = allDocLinks;
            _globalFilter = elementMultiFilter;
            _settingsAR = settingsAR;
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

            var solidIntersectionFactory = GetSolidIntersectionFactory(
                _doc,
                _allDoclinks,
                _settingsAR,
                _globalFilter,
                Logger);

            if (checkRooms)
            {
                var solidRoomIntersectFactory = new SolidRoomIntersectionFactory(_doc, _allDoclinks, _globalFilter)
                { Logger = Logger, TransactionFactory = null };
                var roomCheker = new RoomChecker(
                    _uiDoc, _allDoclinks,
                    _globalFilter,
                    solidIntersectionFactory,
                    solidRoomIntersectFactory)
                {
                    Logger = Logger,
                    //TransactionFactory = trf,
                    WindowMessenger = WindowMessenger
                }.Initiate(mEPCurve);

                if (!roomCheker) { return false; }
            }

           var wallChecker = new WallsChecker(
               _uiDoc,
               _allDoclinks,
               solidIntersectionFactory,
               _settingsAR.WallIntersectionSettings)
            {
                Logger = Logger,
                //TransactionFactory = trf,
                WindowMessenger = WindowMessenger

            };
            return wallChecker.IsValid(mEPCurve);
        }

        private ITIntersectionFactory<Element, Solid> GetSolidIntersectionFactory(
            Document activeDoc,
            IEnumerable<RevitLinkInstance> allDocLinks, 
            DocSettingsBase baseSettings, 
            IElementMultiFilter serviceElementFilter, 
            ILogger logger= null, 
            ITransactionFactory transactionFactory = null)
        {
            var settingsDoc = baseSettings.Docs.FirstOrDefault(d => !d.IsLinked);
            var settingsLinks = baseSettings.Docs.Select(d => d.TryGetLink(allDocLinks)).Where(l => l != null);
            var elementFilterAR = new ElementMutliFilter(settingsDoc, settingsLinks);

            return new SolidElementIntersectionFactory(activeDoc, serviceElementFilter)
            { Logger = logger, TransactionFactory = transactionFactory };
        }
    }
}
