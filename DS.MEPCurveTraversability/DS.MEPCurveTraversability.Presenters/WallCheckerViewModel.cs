using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.MEPCurveTraversability.Interactors;
using Rhino;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UnitSystem = Rhino.UnitSystem;

namespace DS.MEPCurveTraversability.Presenters
{
    public class WallCheckerViewModel : IWallIntersectionSettings
    {
        private static readonly double _mmToFeet =
          RhinoMath.UnitScale(UnitSystem.Millimeters, UnitSystem.Feet);

        private static readonly double _feetToMM =
          RhinoMath.UnitScale(UnitSystem.Feet, UnitSystem.Millimeters);

        private readonly WallIntersectionSettings _settings;

        public WallCheckerViewModel(
            WallIntersectionSettings wallIntersectionSettings, 
            CheckDocsConfigViewModel checkDocsConfig)
        {
            _settings = wallIntersectionSettings;
        }


        public bool CheckOpenings
        {
            get => _settings.CheckOpenings;
            set => _settings.CheckOpenings = value;
        }

        public double InsertsOffset
        {
            get => _settings.InsertsOffset * _feetToMM;
            set => _settings.InsertsOffset = value * _mmToFeet;
        }

        public double JointsOffset
        {
            get => _settings.JointsOffset * _feetToMM;
            set => _settings.JointsOffset = value * _mmToFeet;
        }

        public double NormalAngleLimit
        {
            get => RhinoMath.ToDegrees(_settings.NormalAngleLimit);
            set => _settings.NormalAngleLimit = RhinoMath.ToRadians(value);
        }

        public double OpeningOffset
        {
            get => _settings.OpeningOffset * _feetToMM;
            set => _settings.OpeningOffset = value * _mmToFeet;
        }

        public double WallOffset
        {
            get => _settings.WallOffset * _feetToMM;
            set => _settings.WallOffset = value * _mmToFeet;
        }


        #region Commands

        public ICommand ConfigDocs => new RelayCommand(p =>
        {
            
        }, _ => true);
      

        #endregion
    }
}
