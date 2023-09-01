using System;

namespace ClearDashboard.Wpf.Application.Models;

public class JiraTicketResponse
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public Uri Self { get; set; }
}