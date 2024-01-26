namespace DS.MEPCurveTraversability.Interactors
{
    public interface IWallIntersectionSettings
    {
        bool CheckOpenings { get; set; }
        double InsertsOffset { get; set; }
        double JointsOffset { get; set; }
        double NormalAngleLimit { get; set; }
        double OpeningOffset { get; set; }
        double WallOffset { get; set; }
    }
}