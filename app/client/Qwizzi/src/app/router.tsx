import { Navigate, Outlet, Route, Routes } from 'react-router-dom';
import { AccountPage } from '../features/account/pages/AccountPage';
import { ChangePasswordPage } from '../features/account/pages/ChangePasswordPage';
import { StatisticsPage } from '../features/account/pages/StatisticsPage';
import { AdminUsersPage } from '../features/admin/pages/AdminUsersPage';
import { AdminStatisticsPage } from '../features/admin/pages/AdminStatisticsPage';
import { AdminSecurityPage } from '../features/admin/pages/AdminSecurityPage';
import { AdminQuizzesPage } from '../features/admin/pages/AdminQuizzesPage';
import { ForgotPasswordPage } from '../features/auth/pages/ForgotPasswordPage';
import { LoginPage } from '../features/auth/pages/LoginPage';
import { RegisterPage } from '../features/auth/pages/RegisterPage';
import { GuestRoute } from './guards/GuestRoute';
import { ProtectedRoute } from './guards/ProtectedRoute';
import { AdminRoute } from './guards/AdminRoute';
import { HomePage } from '../features/home/pages/HomePage';
import { BrowsePage } from '../features/quizzes/pages/BrowsePage';
import { QuizManagerPage } from '../features/quizzes/pages/QuizManagerPage';
import { QuizPlayPage } from '../features/quiz-play/pages/QuizPlayPage';
import { LicensePage, NotFoundPage, ErrorPage } from '../features/system/pages/SystemPages';
import { SiteLayout } from '../shared/layout/SiteLayout';

export function Router() {
  return (
    <Routes>
      <Route element={<SiteLayout />}>
        <Route index element={<HomePage />} />
        <Route path="browse" element={<BrowsePage />} />
        <Route path="quiz/:quizId" element={<QuizPlayPage />} />
        <Route
          path="quiz-manager"
          element={
            <ProtectedRoute>
              <QuizManagerPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="account"
          element={
            <ProtectedRoute>
              <AccountPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="account/change-password"
          element={
            <ProtectedRoute>
              <ChangePasswordPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="account/statistics"
          element={
            <ProtectedRoute>
              <StatisticsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="admin"
          element={
            <AdminRoute>
              <Outlet />
            </AdminRoute>
          }
        >
          <Route index element={<Navigate to="users" replace />} />
          <Route path="users" element={<AdminUsersPage />} />
          <Route path="quizzes" element={<AdminQuizzesPage />} />
          <Route path="security" element={<AdminSecurityPage />} />
          <Route path="statistics" element={<AdminStatisticsPage />} />
        </Route>
        <Route path="license" element={<LicensePage />} />
        <Route path="error" element={<ErrorPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
      <Route
        path="/login"
        element={
          <GuestRoute>
            <LoginPage />
          </GuestRoute>
        }
      />
      <Route
        path="/register"
        element={
          <GuestRoute>
            <RegisterPage />
          </GuestRoute>
        }
      />
      <Route
        path="/forgot-password"
        element={
          <GuestRoute>
            <ForgotPasswordPage />
          </GuestRoute>
        }
      />
    </Routes>
  );
}