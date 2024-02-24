using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using Microsoft.Extensions.Options;
using Enviroment.Data;
using Enviroment.Models;
using System.Linq;
using System.Threading.Tasks;
using MailKit;

namespace Enviroment.Services
{
    public class EmailCheckerService
    {
        private readonly EmailSettings _emailSettings;
        private readonly HelpdeskContext _context;

        public EmailCheckerService(IOptions<EmailSettings> emailSettings, HelpdeskContext context)
        {
            _emailSettings = emailSettings.Value;
            _context = context;
        }

        public async Task CheckAndCreateTicketsFromEmailAsync()
        {
            using (var client = new ImapClient())
            {
                // Connect to the IMAP server using SSL
                await client.ConnectAsync(_emailSettings.ImapServer, _emailSettings.ImapPort, useSsl: true);
                await client.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);

                // Open the Inbox folder in read-write mode to be able to mark emails as read
                client.Inbox.Open(FolderAccess.ReadWrite);

                // Search for unread messages
                var uids = client.Inbox.Search(SearchQuery.NotSeen);
                foreach (var uid in uids)
                {
                    var message = client.Inbox.GetMessage(uid);
                    CreateTicketFromEmail(message);

                    // Mark the email as read
                    client.Inbox.AddFlags(uid, MessageFlags.Seen, true);
                }

                // Disconnect from the server
                client.Disconnect(quit: true);
            }
        }

        private void CreateTicketFromEmail(MimeMessage message)
        {
            var ticket = new Ticket
            {
                CustomerName = message.From.Mailboxes.FirstOrDefault()?.Name ?? "Unknown Sender",
                EmailAddress = message.From.Mailboxes.FirstOrDefault()?.Address ?? "Unknown",
                Description = message.TextBody ?? "No content",
                Summary = message.Subject ?? "No subject",
                Status = "Open", // Assuming a default status
                OpenedDate = DateTime.Now,
                LastUpdated = DateTime.Now,
                Category = "General", // Default category, change as needed
                Team = "Unassigned", // Default team, change as needed
                Type = "General Inquiry", // Default type, change as needed
            };

            // Add the new ticket to the context and save changes
            _context.Tickets.Add(ticket);
            _context.SaveChanges();
        }
    }
}
