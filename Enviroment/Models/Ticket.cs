namespace Enviroment.Models
{
    public class Ticket
    {
        public int TicketID { get; set; }
        public string? CustomerName { get; set; }
        public string? EmailAddress { get; set; }
        public string? EmployeeName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string Team { get; set; }
        public string Summary { get; set; }
        public string Type { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? NewNote { get; set; }

    }
}
