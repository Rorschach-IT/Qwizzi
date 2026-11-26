import { AlertCircle, BookOpenCheck, LoaderCircle, RefreshCw } from 'lucide-react';

interface QuizListStateProps {
  loading: boolean;
  error?: string | null;
  hasQuizzes: boolean;
  onRetry?: () => void;
}

export function QuizListState({
  loading,
  error,
  hasQuizzes,
  onRetry,
}: QuizListStateProps) {
  if (loading) {
    return (
      <div className="table-state" role="status">
        <LoaderCircle className="spin" size={23} aria-hidden="true" />
        Loading quizzes…
      </div>
    );
  }

  if (error) {
    return (
      <div className="collection-state" role="alert">
        <div className="collection-state-icon"><AlertCircle size={24} aria-hidden="true" /></div>
        <h2>We couldn’t load the quizzes.</h2>
        <p>{error}</p>
        {onRetry && (
          <button type="button" className="button button-outline" onClick={onRetry}>
            <RefreshCw size={16} aria-hidden="true" /> Try again
          </button>
        )}
      </div>
    );
  }

  if (!hasQuizzes) {
    return (
      <div className="collection-state" role="status">
        <div className="collection-state-icon"><BookOpenCheck size={25} aria-hidden="true" /></div>
        <span className="eyebrow">GOOD THINGS ARE TAKING SHAPE</span>
        <h2>No quizzes to browse just yet.</h2>
        <p>The quiz catalogue will appear here as soon as the quiz API is available.</p>
      </div>
    );
  }

  return null;
}
