using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using DS.MEPTools.OpeningsCreator;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
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

namespace DS.MEPTools.Core
{
    public static class SolidElementRulesFilterSet
    {
        public static Func<(Solid, Element), bool> WallTraversableDirectionRule(Document activeDoc, Vector3d traverseDirection = default)
        {
            return f => IsTraversableDirection(f, activeDoc, traverseDirection);
        }

        public static Func<(Solid, Element), bool> WallConstructionRule(Document doc)
        {
            return f => IsConstructionWall(f, doc);
        }

        public static Func<(Solid, Element), bool> WallOpeningTraversableRule(Document doc,
            Solid solid,
            Element wall,
            ILogger logger,
            ITransactionFactory trf)
        {
            return f => IsOpeningTraversable(doc, solid, wall, logger, trf);
        }



        #region PrivateMethods

        private static bool IsConstructionWall((Solid, Element) f, Document doc)
        {
            var e1 = f.Item1;
            var e2 = f.Item2;

            if (e2 is not Wall wall) { return true; }

            string path;
            if (doc.IsWorkshared)
            {
                var modelPath = doc.GetWorksharingCentralModelPath();
                path = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
            }
            else { path = wall.GetLink(doc)?.Document.PathName ?? doc.PathName; }
            string lastFolderName = Path.GetFileName(Path.GetDirectoryName(path));

            return lastFolderName.Contains("КР") || lastFolderName.Contains("KR");
        }

        private static bool IsTraversableDirection((Solid, Element) f, Document activeDoc, Vector3d traverseDirection)
        {
            var e1 = f.Item1;
            var e2 = f.Item2;

            if (e2 is not Wall wall) { return false; }
            if(!wall.TryGetBasis(activeDoc, out var basis))
            { return false; }

            var at = 1.DegToRad();
            return traverseDirection.IsParallelTo(basis.Y, at) != 0;
        }

        private static bool IsOpeningTraversable(Document doc,
       Solid solid,
       Element wall,
       ILogger logger,
       ITransactionFactory trf)
        {
            var profileCreator = new RectangleSolidProfileCreator(doc)
            {
                Offset = 100.MMToFeet(),
                TransactionFactory = trf,
                Logger = logger
            };
            var wBuilder = new WallOpeningProfileValidator<Solid>(doc, profileCreator)
            {
                InsertsOffset = 500.MMToFeet(),
                WallOffset = 1000.MMToFeet(),
                JointsOffset = 0,
                Logger = logger,
                TransactionFactory = trf
            };

            var mArg = (wall as Wall, solid);

            var b = wBuilder.IsValid(mArg);
            if (!b)
            {
                var sb = new StringBuilder();
                wBuilder.ValidationResults.ToList().ForEach(r => sb.Append(r.ErrorMessage));
                logger?.Information(sb.ToString());
            }

            return b;

            bool Func((Solid, Element) f) => f.Item2 is Wall wall1 && wBuilder.IsValid((wall1, f.Item1));
        }

        #endregion


    }
}
