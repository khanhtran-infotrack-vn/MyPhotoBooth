import { create } from 'zustand';
import api from '../lib/api';

// Helper to get user from localStorage
const getStoredUser = (): User | null => {
  const userJson = localStorage.getItem('user');
  if (!userJson) return null;
  try {
    return JSON.parse(userJson);
  } catch {
    return null;
  }
};

interface User {
  email: string;
  displayName?: string;
}

interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, confirmPassword: string, displayName: string) => Promise<void>;
  logout: () => Promise<void>;
  checkAuth: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  isAuthenticated: !!localStorage.getItem('accessToken'),
  user: getStoredUser(),

  login: async (email: string, password: string) => {
    // Refresh token is now handled by httpOnly cookie
    const { data } = await api.post('/auth/login', { email, password });
    // Store only access token (refresh token is in httpOnly cookie)
    localStorage.setItem('accessToken', data.accessToken);
    const user = { email, displayName: data.displayName };
    localStorage.setItem('user', JSON.stringify(user));
    set({ isAuthenticated: true, user });
  },

  register: async (email: string, password: string, confirmPassword: string, displayName: string) => {
    await api.post('/auth/register', { email, password, confirmPassword, displayName });
  },

  logout: async () => {
    try {
      // Logout endpoint clears the httpOnly cookie server-side
      await api.post('/auth/logout');
    } catch (error) {
      console.error('Logout error:', error);
    }
    // Clear local storage (including old refreshToken from previous implementation)
    localStorage.removeItem('accessToken');
    localStorage.removeItem('user');
    localStorage.removeItem('refreshToken');
    set({ isAuthenticated: false, user: null });
  },

  checkAuth: () => {
    const token = localStorage.getItem('accessToken');
    set({ isAuthenticated: !!token });
  },
}));
