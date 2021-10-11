using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace AddIns
{
    
    public class WallUpdaterApplication : Autodesk.Revit.UI.IExternalApplication
    {
        public Result OnStartup(Autodesk.Revit.UI.UIControlledApplication application)
        {
            // Register wall updater with Revit
            WallUpdater updater = new WallUpdater(application.ActiveAddInId);
            UpdaterRegistry.RegisterUpdater(updater);

            // Change Scope = any Wall element
            ElementClassFilter wallFilter = new ElementClassFilter(typeof(Wall));

            // Change type = element addition
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), wallFilter, Element.GetChangeTypeElementAddition());
            return Result.Succeeded;
        }

        public Result OnShutdown(Autodesk.Revit.UI.UIControlledApplication application)
        {
            WallUpdater updater = new WallUpdater(application.ActiveAddInId);
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
            return Result.Succeeded;
        }
    }
    
    public class WallUpdater : IUpdater
    {
        static AddInId m_appId;
        static UpdaterId m_updaterId;
        WallType m_wallType = null;

        // constructor takes the AddInId for the add-in associated with this updater
        public WallUpdater(AddInId id)
        {
            m_appId = id;
            m_updaterId = new UpdaterId(m_appId, new Guid("48f1f30f-1459-496d-bcd1-b32b713045ce"));
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();

            // Cache the wall type
            if (m_wallType == null)
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                collector.OfClass(typeof(WallType));
                var wallTypes = from element in collector
                                where
                                        element.Name == "Exterior - Brick on CMU"
                                select element;
                if (wallTypes.Count<Element>() > 0)
                {
                    m_wallType = wallTypes.Cast<WallType>().ElementAt<WallType>(0);
                }
            }

            if (m_wallType != null)
            {
                // Change the wall to the cached wall type.
                foreach (ElementId addedElemId in data.GetAddedElementIds())
                {
                    Wall wall = doc.GetElement(addedElemId) as Wall;
                    if (wall != null)
                    {
                        wall.WallType = m_wallType;
                    }
                }
            }
        }

        public string GetAdditionalInformation()
        {
            return "Wall type updater example: updates all newly created walls to a special wall";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FloorsRoofsStructuralWalls;
        }

        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        public string GetUpdaterName()
        {
            return "Wall Type Updater";
        }
    }

}
