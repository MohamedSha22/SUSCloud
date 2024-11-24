using Microsoft.AspNetCore.Mvc;
using SUSCloudTask.Services;
using SUSCloudTask.DTOs;

namespace SUSCloudTask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public ActionResult<List<EmployeeDTO>> GetAllEmployees()
        {
            try
            {
                var employees = _employeeService.GetAllEmployees();
                if (employees == null || employees.Count == 0)
                {
                    return NotFound(new { Message = "No employees found." });
                }
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Unexpected error occurred.", Details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public ActionResult<EmployeeDTO> GetEmployeeById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { Message = "Employee ID must be greater than 0." });

                var employee = _employeeService.GetEmployeeById(id);
                if (employee == null)
                {
                    return NotFound(new { Message = "Employee not found." });
                }
                return Ok(employee);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Unexpected error occurred.", Details = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CreateEmployee([FromBody] EmployeeDTO employee)
        {
            try
            {
                if (employee == null)
                    return BadRequest(new { Message = "Employee data cannot be null." });

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Message = "Invalid data provided.", Errors = ModelState });
                }

                _employeeService.InsertEmployeeWithDetails(employee);
                return Ok(new { Message = "Employee created successfully.", Data = employee });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = "Invalid input.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Unexpected error occurred.", Details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEmployee(int id, [FromBody] EmployeeDTO employee)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { Message = "Employee ID must be greater than 0." });

                if (employee == null)
                    return BadRequest(new { Message = "Employee data cannot be null." });

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Message = "Invalid data provided.", Errors = ModelState });
                }

                var existingEmployee = _employeeService.GetEmployeeById(id);
                if (existingEmployee == null)
                {
                    return NotFound(new { Message = "Employee not found." });
                }

                _employeeService.UpdateEmployeeWithDetails(id, employee);
                return Ok(new { Message = "Employee updated successfully.", Data = employee });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = "Invalid input.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Unexpected error occurred.", Details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteEmployee(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { Message = "Employee ID must be greater than 0." });

                var employee = _employeeService.GetEmployeeById(id);
                if (employee == null)
                {
                    return NotFound(new { Message = "Employee not found." });
                }

                _employeeService.DeleteEmployeeWithDetails(id);
                return Ok(new { Message = "Employee deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Unexpected error occurred.", Details = ex.Message });
            }
        }
    }
}
