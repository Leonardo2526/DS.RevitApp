using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using DS.MEPCurveTraversability.Interactors.Settings;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.MEPCurveTraversability.Interactors
{
    /// <summary>
    /// An object to store traversability settings.
    /// </summary>
    public class DocIndexSettings : 
        Dictionary<int, IEnumerable<ITraversabilitySettings>>
    {
        /// <summary>
        /// Get traversability settings.
        /// </summary>
        /// <param name="activeDoc"></param>
        /// <param name="application"></param>
        /// <param name="getARSettings"></param>
        /// <param name="getKRSettings"></param>
        /// <returns>
        /// Exist or new created settings for <paramref name="activeDoc"/>.
        /// </returns>
        public IEnumerable<ITraversabilitySettings> GetSettings(
            Document activeDoc,
            Application application,
            Func<DocSettingsAR> getARSettings,
            Func<DocSettingsKR> getKRSettings)
        {
            var settings = TryGetValue(activeDoc, application);
            if (settings == null)
            {
                var docSettingsAR = getARSettings.Invoke();
                var docSettingsKR = getKRSettings.Invoke();
                settings = new List<ITraversabilitySettings>()
                { docSettingsAR, docSettingsKR };
                Add(activeDoc.GetHashCode(), settings);
            }

            return settings;
        }

        public IEnumerable<ITraversabilitySettings> TryGetValue(
            Document activeDoc,
            Application application)
        {
            RemoveInvalid(application);
            if (TryGetValue(activeDoc.GetHashCode(), 
                out IEnumerable<ITraversabilitySettings> settings))
            {
                return settings;
            }

            return null;
        }

        private void RemoveInvalid(Application application)
        {
            var appActiveDocCodes = application.Documents.
                Cast<Document>().
                Where(d => !d.IsLinked).ToList().
                Select(d => d.GetHashCode());
            var keys = new List<int>();
            Keys.ForEach(keys.Add);
            foreach (var k in keys)
            {
                if (!appActiveDocCodes.Contains(k))
                { Remove(k); }
            }
        }
    }
}
