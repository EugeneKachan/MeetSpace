export interface User {
  id: string;
  name: string;
  email: string;
  role: 'Admin' | 'Manager' | 'Employee';
  isActive: boolean;
}

export interface AuthResponse {
  token: string;
  user: User;
}
