using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddID
{
    class ParameterUpdater
    {
        UpdaterId _uid;
        public ParameterUpdater(Guid guid)
        {
            _uid = new UpdaterId(new AddInId(
                new Guid("c1f5f009-8ba9-4f1d-b0fb-ba41a0f69942")), // addin id
                guid); // updater id
        }

        public void Execute(UpdaterData data)
        {
            Func<ICollection<ElementId>, string> toString = ids => ids.Aggregate("", (ss, id) => ss + "," + id).TrimStart(',');
            var sb = new StringBuilder();
            sb.AppendLine("added:" + toString(data.GetAddedElementIds()));
            sb.AppendLine("modified:" + toString(data.GetModifiedElementIds()));
            sb.AppendLine("deleted:" + toString(data.GetDeletedElementIds()));
            TaskDialog.Show("Changes", sb.ToString());
        }

        public string GetAdditionalInformation()
        {
            return "N/A";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FreeStandingComponents;
        }

        public UpdaterId GetUpdaterId()
        {
            return _uid;
        }

        public string GetUpdaterName()
        {
            return "ParameterUpdater";
        }
    }

}
