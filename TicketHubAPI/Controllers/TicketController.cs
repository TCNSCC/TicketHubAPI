using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using TicketHubAPI.Models;

namespace TicketHubAPI.Controllers
{
    [Route("api/tickets")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly QueueClient _queueClient;
        private readonly IConfiguration _configuration;

        public TicketController(IConfiguration configuration)
        {
            _configuration = configuration;
            
            string connectionString = _configuration["AzureStorageConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                
                throw new ArgumentException("Azure Storage Connection String is missing or invalid.");
            }
            _queueClient = new QueueClient(connectionString, "tickethub");

      
            _queueClient.CreateIfNotExists();
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello from Tickets Controller - GET");
        }

        [HttpPost("checkout")]
        public IActionResult PurchaseTicket([FromBody] TicketPurchase ticket)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                
                string ticketJson = JsonSerializer.Serialize(ticket);
            
                _queueClient.SendMessage(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(ticketJson)));
                return Ok(new { message = "Ticket purchase request received." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing request", error = ex.Message });
            }
        }
    }
}
