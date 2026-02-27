export type UserRole = 'Admin' | 'OfficeManager' | 'Employee';

export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
}

/** Raw token response from POST /connect/token (OAuth2 ROPC) */
export interface TokenResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  refresh_token?: string;
}

/** Decoded JWT claims as issued by OpenIddict */
export interface JwtClaims {
  sub: string;           // user ID
  email: string;
  name: string;          // username (equals email by default)
  given_name: string;    // FirstName
  family_name: string;   // LastName
  role: string | string[];
  exp: number;
}
