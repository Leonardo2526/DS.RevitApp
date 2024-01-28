using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using DS.MEPCurveTraversability.Interactors.Settings;
using OLMP.RevitAPI.Core.Extensions;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPCurveTraversability.Interactors
{
    public class WallIntersectionValidatorFactory : IValidatorFactory<MEPCurve>
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _allLoadedLinks;
        private readonly IElementMultiFilter _elementMultiFilter;
        private readonly IEnumerable<Document> _validatableDocs;
        private readonly IWallIntersectionSettings _wallIntersectionSettings;

        public WallIntersectionValidatorFactory(
            UIDocument uiDoc,
            IEnumerable<RevitLinkInstance> allLoadedLinks,
            IElementMultiFilter elementMultiFilter,
            IEnumerable<Document> validatableDocs, 
            IWallIntersectionSettings wallIntersectionSettings)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _allLoadedLinks = allLoadedLinks;
            _elementMultiFilter = elementMultiFilter;
            _validatableDocs = validatableDocs;
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
            var solidIntersectionFactory = GetSolidIntersectionFactory(
              _doc,
              _allLoadedLinks,
              _validatableDocs,
              _elementMultiFilter,
              Logger);

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


        private ITIntersectionFactory<Element, Solid> GetSolidIntersectionFactory(
            Document activeDoc,
            IEnumerable<RevitLinkInstance> allDocLinks,
            IEnumerable<Document> validatableDocs,
            IElementMultiFilter serviceElementFilter,
            ILogger logger = null,
            ITransactionFactory transactionFactory = null)
        {
            var settingsDoc = validatableDocs.FirstOrDefault(d => !d.IsLinked);
            var settingsLinks = validatableDocs.Select(d => d.TryGetLink(allDocLinks)).Where(l => l != null);
            var elementFilterAR = new ElementMutliFilter(settingsDoc, settingsLinks);

            return new SolidElementIntersectionFactory(activeDoc, serviceElementFilter)
            { Logger = logger, TransactionFactory = transactionFactory };
        }
    }
}
