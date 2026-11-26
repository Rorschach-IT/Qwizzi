import { useEffect, useState } from 'react';
import { ChartNoAxesColumnIncreasing, LoaderCircle, Trophy } from 'lucide-react';
import { Link } from 'react-router-dom';
import { quizzesService, type QuizStatistics } from '../../quizzes/api/quizzesService';

function percentage(value: number) {
  return `${Math.round(value)}%`;
}

function formatDate(value: string) {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? '—' : new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(date);
}

export function StatisticsPage() {
  const [statistics, setStatistics] = useState<QuizStatistics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let active = true;
    void quizzesService.statistics()
      .then((result) => { if (active) setStatistics(result); })
      .catch((requestError: unknown) => { if (active) setError(requestError instanceof Error ? requestError.message : 'Unable to load statistics.'); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, []);

  return (
    <section className="page-container">
      <div className="page-heading">
        <span className="eyebrow">YOUR PROGRESS</span>
        <h1>Statistics</h1>
        <p>A little progress adds up to a lot of learning.</p>
      </div>
      {error && <div className="notice error-notice" role="alert">{error}</div>}
      {loading ? (
        <div className="collection-state" role="status"><LoaderCircle className="spin" size={24} /><p>Loading your statistics…</p></div>
      ) : !statistics || statistics.totalAttempts === 0 ? (
        <div className="collection-state">
          <div className="collection-state-icon"><ChartNoAxesColumnIncreasing size={25} aria-hidden="true" /></div>
          <span className="eyebrow">YOUR JOURNEY STARTS HERE</span>
          <h2>No activity to show yet.</h2>
          <p>Take an approved quiz and your attempts and scores will appear here.</p>
          <Link className="button button-primary" to="/browse">Browse quizzes</Link>
        </div>
      ) : (
        <>
          <div className="statistics-summary-grid">
            <article className="statistics-summary-card"><span>Total attempts</span><strong>{statistics.totalAttempts}</strong><small>{statistics.completedAttempts} completed</small></article>
            <article className="statistics-summary-card"><span>Average score</span><strong>{percentage(statistics.averageScorePercent)}</strong><small>Across completed attempts</small></article>
            <article className="statistics-summary-card statistics-best-card"><span><Trophy size={15} /> Best score</span><strong>{percentage(statistics.bestScorePercent)}</strong><small>Your top completed result</small></article>
          </div>
          <section className="table-card statistics-table-card">
            <div className="table-toolbar"><div><h2>Quiz history</h2><p>Your attempts, scores, and completion dates.</p></div></div>
            <div className="table-scroll">
              <table className="statistics-table">
                <thead><tr><th>Quiz</th><th>Date</th><th>Result</th><th>Status</th></tr></thead>
                <tbody>{statistics.attempts.map((attempt) => (
                  <tr key={attempt.attemptId}>
                    <td><strong>{attempt.quizTitle}</strong></td>
                    <td>{formatDate(attempt.startedAt)}</td>
                    <td>{attempt.completedAt ? `${attempt.score} / ${attempt.totalQuestions} (${percentage(attempt.totalQuestions ? attempt.score / attempt.totalQuestions * 100 : 0)})` : '—'}</td>
                    <td><span className={`status-pill${attempt.completedAt ? ' status-active' : ' status-inactive'}`}><i />{attempt.completedAt ? 'Complete' : 'In progress'}</span></td>
                  </tr>
                ))}</tbody>
              </table>
            </div>
          </section>
        </>
      )}
    </section>
  );
}
