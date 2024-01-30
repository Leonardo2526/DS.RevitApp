namespace DS.MEPCurveTraversability.Interactors
{
    /// <summary>
    /// Settings to check objects intersections with walls.
    /// </summary>
    public interface IWallIntersectionSettings : IWallOpeningSettings
    {
        /// <summary>
        /// Max angle between wall's Y face normal and intersection direction. 
        /// </summary>
        double NormalAngleLimit { get; set; }
    }
}