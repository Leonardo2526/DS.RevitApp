using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using DS.MEPCurveTraversability.Interactors.Settings;
using OLMP.RevitAPI.Core.Extensions;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using Serilog;
using Serilog.Core;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPCurveTraversability.Interactors
{
    public class WallIntersectionValidatorFactory : IValidatorFactory<MEPCurve>
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _allLoadedLinks;
        private readonly IElementMultiFilter _localFilter;
        private readonly IWallIntersectionSettings _wallIntersectionSettings;

        public WallIntersectionValidatorFactory(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> allLoadedLinks,
            IElementMultiFilter localFilter,
            IWallIntersectionSettings wallIntersectionSettings)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _allLoadedLinks = allLoadedLinks;
            _localFilter = localFilter;
            _wallIntersectionSettings = wallIntersectionSettings;
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

        public IValidator<MEPCurve> GetValidator()
        {
            var solidIntersectionFactory = new SolidElementIntersectionFactory(_doc, _localFilter)
            { Logger = Logger, TransactionFactory = TransactionFactory };

            return new WallsChecker(
              _uiDoc,
              _allLoadedLinks,
              solidIntersectionFactory,
              _wallIntersectionSettings)
            {
                Logger = Logger,
                TransactionFactory = TransactionFactory,
                WindowMessenger = WindowMessenger
            };
        }
    }
}
