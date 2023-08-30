using System;

namespace ClearDashboard.Wpf.Application.Models;

public class JiraTicketResponse
{
    public int Id { get; set; }
    public string Key { get; set; }
    public Uri Self { get; set; }
}