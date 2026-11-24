import { useEffect } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { useLocation } from 'react-router-dom';
import { Providers } from './Providers';
import { Router } from './Router';
import './App.css';

function PageTitle() {
  const { pathname } = useLocation();

  useEffect(() => {
    const titles: Record<string, string> = {
      '/': 'Home',
      '/browse': 'Browse quizzes',
      '/quiz-manager': 'Quiz Manager',
      '/login': 'Sign in',
      '/register': 'Create an account',
      '/forgot-password': 'Reset password',
      '/account': 'Account settings',
      '/account/statistics': 'Your statistics',
      '/admin/users': 'Manage users',
      '/admin/quizzes': 'Manage quizzes',
      '/admin/statistics': 'Admin statistics',
      '/license': 'License & agreement',
      '/error': 'Something went wrong',
    };

    document.title = `${titles[pathname] ?? (pathname === '/admin' ? 'Administration' : 'Page not found')} | Qwizzi`;
  }, [pathname]);

  return null;
}

export default function App() {
  return (
    <Providers>
      <BrowserRouter>
        <PageTitle />
        <Router />
      </BrowserRouter>
    </Providers>
  );
}
