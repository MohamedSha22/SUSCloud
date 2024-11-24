import { Routes } from '@angular/router';
import { EmployeeFormComponent } from './Components/employee-form/employee-form.component';
import { EmployeeListComponent } from './Components/employee-list/employee-list.component';
import { EditEmployeeComponent } from './Components/edit-employee/edit-employee.component';
import { ErrorComponent } from './Components/error/error.component';

export const routes: Routes = [
  { path: '', redirectTo: '/employees', pathMatch: 'full' },
  { path: 'employees', component: EmployeeListComponent },
  { path: 'employees/create', component: EmployeeFormComponent },
  { path: 'employees/edit/:id', component: EditEmployeeComponent }, // Route for editing
  { path: '**', component: ErrorComponent }
];
