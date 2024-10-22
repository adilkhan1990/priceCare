namespace PriceCare.Web.Models
{
    public class DimensionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string ColorCode { get; set; }
    }

    public enum EventTypes
    {
        //Nothing
        NotSpecified = 23,
        NoEvent = 44,
        //Cuts
        ExoCut = 20,
        RvlCut = 22,
        TcCut = 46,
        OthCut = 27,
        IncCut = 29,
        //Launch Target
        LaunchTarget = 24,
        //Launch Launch Rule
        LaunchLaunchRule = 25,
        //Launch isDefault Rule
        LaunchDefaultRule = 26,
        //Review
        Review = 28,
    }

    public enum MathTypes
    {
        Average = 33,
        Lowest = 34,
        Maximum = 35,
        WeightedAverage	= 36,
        Median = 37,
        NotSpecified = 38,
        Quartile = 53
    }

    public enum WeightType
    {
        NotSpecified = 48
    }
}
