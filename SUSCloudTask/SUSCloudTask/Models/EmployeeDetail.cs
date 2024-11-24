using System;
using System.Collections.Generic;

namespace SUSCloudTask.Models;

public partial class EmployeeDetail
{
    public int DetailId { get; set; }

    public int EmployeeId { get; set; }

    public string? Project { get; set; }

    public string? Address { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
