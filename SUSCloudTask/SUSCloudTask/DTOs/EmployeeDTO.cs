using SUSCloudTask.Helpers;
using System.ComponentModel.DataAnnotations;

namespace SUSCloudTask.DTOs
{
    public class EmployeeDTO
    {
        public int id {  get; set; }

        [Required(ErrorMessage = "Employee Name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Employee Name must be between 3 to 100 characters.")]
        [Display(Name = "Employee Name")]
        [RegularExpression("^[A-Za-z\\s-]+$", ErrorMessage = "Employee Name must contain only alphabetic characters, spaces, or hyphens.")]
        public required string name { get; set; }

        [Required(ErrorMessage = "Position is required.")]
        [Display(Name = "Position")]
        [StringLength(50, ErrorMessage = "Position cannot exceed 50 characters.")]
        public required string position { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        [Display(Name = "Department")]
        [StringLength(50, ErrorMessage = "Department cannot exceed 50 characters.")]
        public required string department { get; set; }

        [Required(ErrorMessage = "Salary is required.")]
        [Range(0, 100000, ErrorMessage = "Salary must be a positive value between 0 and 100,000.")]
        [Display(Name = "Salary")]
        public decimal salary { get; set; }

        [Required(ErrorMessage = "Project is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Project must be between 3 to 100 characters.")]
        [Display(Name = "Project")]
        public required string project { get; set; }

        [Display(Name = "Address")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string? address { get; set; }

        [Required(ErrorMessage = "Start Date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Start Date must be a valid date.")]
        [Display(Name = "Start Date")]
        public DateTime? startDate { get; set; }

        [DataType(DataType.Date, ErrorMessage = "End Date must be a valid date.")]
        [Display(Name = "End Date")]
        [DateGreaterThan("startDate", ErrorMessage = "End Date must be greater than Start Date.")]
        public DateTime? endDate { get; set; }
    }

}
