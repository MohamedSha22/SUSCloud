using System;
using System.Collections.Generic;

namespace SUSCloudTask.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public byte[]? Name { get; set; }

    public string? Position { get; set; }

    public string? Department { get; set; }

    public byte[]? Salary { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<EmployeeDetail> EmployeeDetails { get; set; } = new List<EmployeeDetail>();
}
