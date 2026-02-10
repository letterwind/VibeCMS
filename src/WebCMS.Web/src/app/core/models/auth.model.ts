export interface LoginRequest {
  account: string;
  password: string;
  captcha: string;
  captchaToken: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  user: UserDto;
  expiresAt: Date;
}

export interface UserDto {
  id: number;
  account: string;
  displayName: string;
  isPasswordExpired: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface CaptchaResponse {
  imageBase64: string;
  token: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
