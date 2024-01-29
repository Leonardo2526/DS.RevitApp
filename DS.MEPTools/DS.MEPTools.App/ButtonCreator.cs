using Autodesk.Revit.UI;
using DS.MEPCurveTraversability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DS.MEPTools.App
{
    internal class ButtonCreator
    {      
        private static readonly string _assemblyName = Assembly.GetAssembly(typeof(ExternalCommand)).Location;
        private static readonly string _uriString = @"pack://application:,,,/DS.MEPTools.App;component/Resources";

        public string LargeImageName { get; set; }
        public string ImageName { get; set; }
        public string ButtonName { get; set; }
        public string ButtonText { get; set; }
        public string ClassName { get; set; }

        public PushButtonData Create()
        {
            var largeImageUri = String.IsNullOrEmpty(LargeImageName) ? null : "/" + LargeImageName;
            var imageUri = String.IsNullOrEmpty(ImageName) ? null : "/" + ImageName;

            var uriSource1 = new Uri(_uriString + largeImageUri);
            var image1 = new BitmapImage(uriSource1);
            var uriSource2 = new Uri(_uriString + imageUri);
            var image2 = new BitmapImage(uriSource2);
            var button =
                new PushButtonData(ButtonName, ButtonText, _assemblyName, ClassName)
                {
                    LargeImage = image1,
                    Image = image2
                };
            return button;
        }
    }
}
