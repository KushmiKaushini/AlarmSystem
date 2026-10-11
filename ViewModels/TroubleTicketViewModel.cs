using AlarmSystem.Models;
using System;
using System.Collections.Generic;

namespace AlarmSystem.ViewModels
{
    public class TroubleTicketViewModel
    {
        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public List<TroubleTicket> Tickets { get; set; }
            = new List<TroubleTicket>();
    }
}