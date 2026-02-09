import { create } from 'zustand';
import api from '../lib/api';

interface AuthState {
  isAuthenticated: boolean;
  user: { email: string } | null;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, confirmPassword: string, displayName: string) => Promise<void>;
  logout: () => Promise<void>;
  checkAuth: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  isAuthenticated: !!localStorage.getItem('accessToken'),
  user: null,

  login: async (email: string, password: string) => {
    const { data } = await api.post('/auth/login', { email, password });
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    set({ isAuthenticated: true, user: { email } });
  },

  register: async (email: string, password: string, confirmPassword: string, displayName: string) => {
    await api.post('/auth/register', { email, password, confirmPassword, displayName });
  },

  logout: async () => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      try {
        await api.post('/auth/logout', { refreshToken });
      } catch (error) {
        console.error('Logout error:', error);
      }
    }
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    set({ isAuthenticated: false, user: null });
  },

  checkAuth: () => {
    const token = localStorage.getItem('accessToken');
    set({ isAuthenticated: !!token });
  },
}));
