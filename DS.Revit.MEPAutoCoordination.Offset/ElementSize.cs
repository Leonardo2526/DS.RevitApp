using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace DS.CollisionsElliminator
{

    /// <summary>
    /// Get sizes of Elem1 and Elem2 and set class properties
    /// </summary>
    class ElementSize
    {
        readonly MEPCurve Elem1MEPCurve;
        readonly MEPCurve Elem2MEPCurve;

        public ElementSize(MEPCurve el1MEPCurve, MEPCurve el2MEPCurve)
        {
            Elem1MEPCurve = el1MEPCurve;
            Elem2MEPCurve = el2MEPCurve;
        }

        void GetElementSizes(MEPCurve elMEPCurve,
            out double elementWidth, out double elementHeight, out bool elementIsRectangular)
        {
            string type = elMEPCurve.GetType().ToString();

            //Get element sizes
            if (type.Contains("Pipe"))
            {
                Pipe pipe = elMEPCurve as Pipe;

                double elSize = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();
                elementWidth = elSize;
                elementHeight = elSize;

                elementIsRectangular = false;
            }
            else
            {
                try
                {
                    elementIsRectangular = true;

                    elementWidth = elMEPCurve.Width;
                    elementHeight = elMEPCurve.Height;
                }
                catch
                {
                    elementIsRectangular = false;

                    elementWidth = elMEPCurve.Diameter;
                    elementHeight = elementWidth;
                }

            }
        }

        public void GetSizes()
        {
            //Get element1 sizes
            GetElementSizes(Elem1MEPCurve,
                out double element1Width, out double element1Height, out bool element1IsRectangular);
            Data.Elem1Width = element1Width;
            Data.Elem1Height = element1Height;
            Data.Elem1IsRectangular = element1IsRectangular;

            //Get element2 sizes
            GetElementSizes(Elem2MEPCurve,
                out double element2Width, out double element2Height, out bool element2IsRectangular);
            Data.Elem2Width = element2Width;
            Data.Elem2Height = element2Height;
            Data.Elem2IsRectangular = element2IsRectangular;
        }
    }
}
