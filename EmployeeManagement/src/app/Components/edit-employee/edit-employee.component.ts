import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Employee } from '../../Models/employee';
import { EmployeeService } from '../../Services/employee.service';

@Component({
  selector: 'app-edit-employee',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './edit-employee.component.html',
  styles: [],
})
export class EditEmployeeComponent implements OnInit {
  employee: Employee = {
    id: 0,
    name: '',
    position: '',
    department: '',
    salary: 0,
    project: '',
    address: '',
    startDate: new Date,
    endDate: new Date,
  };
  isLoading: boolean = true;
  errorMessage: string | null = null;
  validationErrors: { [key: string]: string } = {}; 

  constructor(
    private route: ActivatedRoute,
    private employeeService: EmployeeService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadEmployee();
  }

  private loadEmployee(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (isNaN(id) || id <= 0) {
      this.errorMessage = 'Invalid employee ID.';
      this.isLoading = false;
      return;
    }

    this.employeeService.getEmployeeById(id).subscribe({
      next: (data) => {
        this.employee = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching employee:', error);
        this.errorMessage = 'Failed to load employee details.';
        this.isLoading = false;
      },
    });
  }

  submitForm(employeeForm: NgForm): void {
    if (employeeForm.invalid) {
      // Show validation errors for required fields
      return;
    }

    this.employeeService.updateEmployee(this.employee.id, this.employee).subscribe({
      next: () => {
        alert('Employee updated successfully!');
        this.router.navigate(['/employees']);
      },
      error: (error) => {
        console.error('Error updating employee:', error);

        if (error.status === 400 && error.error.errors) {
          // Server-side validation errors
          this.validationErrors = error.error.errors;
        } else {
          this.errorMessage = 'An unexpected error occurred. Please try again.';
        }
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/employees']);
  }
}
