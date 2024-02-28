namespace Enviroment.Models
{
    public class CategoryKPI
    {
        public string CategoryName { get; set; }
        public double? AverageSolveTime { get; set; }
        public double AverageFirstResponseTime { get; set; } // in minutes
        public int TicketVolume { get; set; }
        public int TicketBacklog { get; set; }
        public double IncidentVsServiceRequestRatio { get; set; }
        public int ServiceRequestCount { get; set; }
        public int IncidentCount { get; set; }

    }
}
