using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.MEPTools.OpeningsCreator;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Various;
using System.Threading.Tasks;

namespace DS.MEPTools
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application application = uiapp.Application;
            UIDocument uiDoc = uiapp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            var trf = new ContextTransactionFactory(doc, RevitLib.Utils.RevitContextOption.Inside);
            var profileCreator = new RectangleProfileCreator(doc)
            {
                Offset = 100.MMToFeet(), 
                TransactionFactory = trf
            };
            
            var factory = new OpeningProfileFactory(uiDoc, profileCreator, trf);

            var profile = factory.CreateProfile();
            if(profile == null) { return Result.Cancelled; }
            factory.ShowAsync(profile);

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
