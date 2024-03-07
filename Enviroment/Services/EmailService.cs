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

        public async Task SendEmailAsync(string email, string subject, string message, int ticketId, string ticketType)
        {
            // Modify the subject line to include ticket ID and type
            string modifiedSubject = $"[Ticket ID: {ticketId}, Type: {ticketType}] {subject}";
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.Email));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
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

