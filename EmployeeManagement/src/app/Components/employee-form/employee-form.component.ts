import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms'; // For template-driven forms
import { Employee } from '../../Models/employee'; // Import Employee interface
import { EmployeeService } from '../../Services/employee.service'; // Import EmployeeService
import { Router } from '@angular/router'; // For navigation after form submission

@Component({
  selector: 'app-employee-form',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './employee-form.component.html',
  styles: []
})
export class EmployeeFormComponent {
  // Initialize employee 
  employee: Employee = {
    id: 0,
    name: '',
    position: '',
    department: '',
    salary: 0,
    project: '',
    address: '',
    startDate: new Date(),
    endDate: new Date()
  };

  isSubmitting: boolean = false; // To prevent multiple submissions

  constructor(private employeeService: EmployeeService, private router: Router) {}

  // Handle form submission
  submitForm(): void {
    this.isSubmitting = true; // Disable form during submission
    this.employeeService.createEmployee(this.employee).subscribe({
      next: () => {
        alert('Employee created successfully!');
        this.router.navigate(['/employees']); // Navigate to employee list
      },
      error: (error) => {
        console.error('Error creating employee:', error);
        alert('Failed to create employee. Please try again.');
      },
      complete: () => {
        this.isSubmitting = false; // Re-enable the form
      }
    });
  }

  // Handle cancel action
  cancel(): void {
    this.router.navigate(['/employees']); // Navigate back to the employee list
  }
}
