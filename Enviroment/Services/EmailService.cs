using Enviroment.Data;
using Enviroment.Models;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;

namespace Enviroment.Services
{
    public class EmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly HelpdeskContext _context;

        public EmailService(IOptions<EmailSettings> emailSettings, HelpdeskContext context)
        {
            _emailSettings = emailSettings.Value;
            _context = context;
        }

  




        // Method to parse email and create a ticket
        private void CreateTicketFromEmail(MimeMessage message)
        {
            var ticket = new Ticket
            {
                CustomerName = message.From.Mailboxes.FirstOrDefault()?.Name ?? "Unknown Sender",
                EmailAddress = message.From.Mailboxes.FirstOrDefault()?.Address,
                Description = message.TextBody,
                Summary = message.Subject,
                Status = "Open", 
                OpenedDate = DateTime.Now,
                LastUpdated = DateTime.Now
                
            };

            _context.Tickets.Add(ticket);
            _context.SaveChanges();
        }
    }
}

