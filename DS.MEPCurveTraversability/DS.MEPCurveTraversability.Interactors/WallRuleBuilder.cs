using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using OLMP.RevitAPI.Tools.OpeningsCreator;
using OLMP.RevitAPI.Tools;
using OLMP.RevitAPI.Tools.Creation.Transactions;
using OLMP.RevitAPI.Tools.Extensions;
using MoreLinq;
using Rhino.Geometry;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DS.MEPCurveTraversability.Core;

namespace DS.MEPCurveTraversability.Interactors
{
    public class WallRuleBuilder
    {
        private readonly Document _doc;
        private readonly IEnumerable<RevitLinkInstance> _links;
        private readonly WallIntersectionSettings _wallIntersectionSettings;

        public WallRuleBuilder(
            Document doc,
            IEnumerable<RevitLinkInstance> links,
            WallIntersectionSettings wallIntersectionSettings)
        {
            _doc = doc;
            _links = links;
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

        //public static Func<(Solid, Element), bool> WallTraversableDirectionRule(
        //    Document activeDoc,
        //    IEnumerable<RevitLinkInstance> links,
        //    Vector3d traverseDirection = default)
        //{
        //    return f => IsTraversableDirection(f, activeDoc, links, traverseDirection);
        //}

        //public static Func<(Solid, Element), bool> WallConstructionRule()
        //{
        //    return f => IsConstructionWall(f, doc, links);
        //}

        //public static Func<(Solid, Element), bool> WallOpeningTraversableRule(Document doc, IEnumerable<RevitLinkInstance> links,
        //    Solid solid,
        //    ILogger logger,
        //    ITransactionFactory trf)
        //{
        //    return f => IsOpeningTraversable(doc, links, solid, f.Item2 as Wall, logger, trf);
        //}

        public bool IsConstructionWall((Solid, Element) f)
        {
            var e1 = f.Item1;
            var e2 = f.Item2;

            if (e2 is not Wall wall) { return true; }

            string path;
            if (_doc.IsWorkshared)
            {
                var modelPath = _doc.GetWorksharingCentralModelPath();
                path = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
            }
            else { path = wall.TryFindLink(_links)?.Document.PathName ?? _doc.PathName; }
            string lastFolderName = Path.GetFileName(Path.GetDirectoryName(path));

            return lastFolderName.Contains("КР") || lastFolderName.Contains("KR");
        }

        public bool IsTraversableDirection((Solid, Element) f, Vector3d traverseDirection)
        {
            var e1 = f.Item1;
            var e2 = f.Item2;

            if (e2 is not Wall wall) { return false; }
            if (!wall.TryGetBasis(_doc, _links, out var basis))
            { return false; }

            var at = 1.DegToRad();
            return traverseDirection.IsParallelTo(basis.Y, at) != 0;
        }

        public bool IsOpeningTraversable(Solid solid,
            Element wall)
        {
            var profileCreator = new RectangleSolidProfileCreator(_doc, _links)
            {
                Offset = _wallIntersectionSettings.OpeningOffset,
                TransactionFactory = TransactionFactory,
                Logger = Logger
            };
            var wBuilder = new WallOpeningProfileValidator<Solid>(_doc, _links, profileCreator)
            {
                InsertsOffset = _wallIntersectionSettings.InsertsOffset,
                WallOffset = _wallIntersectionSettings.WallOffset,
                JointsOffset = _wallIntersectionSettings.JointsOffset,
                Logger = Logger,
                TransactionFactory = TransactionFactory
            };

            var mArg = (wall as Wall, solid);

            var b = wBuilder.IsValid(mArg);
            if (!b)
            {
                var sb = new StringBuilder();
                wBuilder.ValidationResults.ToList().ForEach(r => sb.Append(r.ErrorMessage));
                Logger?.Information(sb.ToString());
            }

            return b;
        }

    }
}
