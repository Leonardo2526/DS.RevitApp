using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Openings;
using DS.RevitLib.Utils.Various;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;

namespace DS.MEPTools.OpeningsCreator
{
    internal class OpeningProfileFactory
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly IOpeningProfileCreator _openingProfileCreator;
        private readonly ITransactionFactory _transactionFactory;

        public OpeningProfileFactory(
            UIDocument uiDoc, 
            IOpeningProfileCreator openingProfileCreator, ITransactionFactory transactionFactory
            )
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _openingProfileCreator = openingProfileCreator;
            _transactionFactory = transactionFactory;
        }
      

        public CurveLoop CreateProfile()
        {
            var mEPCurve = new ElementSelector(_uiDoc).Pick() as MEPCurve ?? throw new ArgumentNullException();
            var wall = new ElementSelector(_uiDoc).Pick() as Wall ?? throw new ArgumentNullException();

            return _openingProfileCreator.CreateProfile(wall, mEPCurve);
        }

        public async Task ShowAsync(CurveLoop profile)
        {
            var curves =  profile.ToList();
            await _transactionFactory.CreateAsync(() => curves.ForEach(c => c.Show(_doc)), "showProfile");
        }
    }
}
