import { useEffect, useState } from 'react';
import { Check, LoaderCircle, RefreshCw, ShieldAlert, X } from 'lucide-react';
import { ApiError } from '../../../core/api/apiError';
import { imageUrl, type QuizSummary } from '../../quizzes/api/quizzesService';
import { adminQuizzesService } from '../api/adminQuizzesService';

const statusNames: Record<number, string> = {
  1: 'Draft',
  2: 'Awaiting review',
  3: 'Approved',
  4: 'Blocked',
};

export function AdminQuizzesPage() {
  const [quizzes, setQuizzes] = useState<QuizSummary[]>([]);
  const [reasons, setReasons] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(true);
  const [pendingId, setPendingId] = useState('');
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');

  async function load(showLoading = true) {
    if (showLoading) setLoading(true);
    try {
      setQuizzes(await adminQuizzesService.list());
    } catch (requestError) {
      setError(requestError instanceof ApiError ? requestError.message : 'Unable to load quizzes for review.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    let active = true;
    void adminQuizzesService.list()
      .then((result) => { if (active) setQuizzes(result); })
      .catch((requestError: unknown) => {
        if (active) setError(requestError instanceof ApiError ? requestError.message : 'Unable to load quizzes for review.');
      })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, []);

  async function moderate(quiz: QuizSummary, status: 3 | 4) {
    const reason = reasons[quiz.quizId]?.trim() ?? '';
    if (status === 4 && !reason) {
      setError(`Add a short reason before blocking “${quiz.title}”.`);
      return;
    }
    setPendingId(quiz.quizId);
    setError('');
    setNotice('');
    try {
      await adminQuizzesService.setStatus(quiz.quizId, status, reason);
      setNotice(status === 3 ? `“${quiz.title}” is approved.` : `“${quiz.title}” was blocked.`);
      await load();
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Unable to update quiz status.');
    } finally {
      setPendingId('');
    }
  }

  return (
    <section className="page-container admin-page">
      <div className="page-heading page-heading-row">
        <div><span className="eyebrow">ADMINISTRATION</span><h1>Quiz review</h1><p>Review every submission and manage its publication status.</p></div>
        <button type="button" className="icon-button refresh-button" aria-label="Refresh quizzes" disabled={loading} onClick={() => { setError(''); void load(); }}><RefreshCw size={17} /></button>
      </div>
      {notice && <div className="notice success-notice" role="status">{notice}</div>}
      {error && <div className="notice error-notice" role="alert">{error}</div>}
      {loading ? (
        <div className="collection-state" role="status"><LoaderCircle className="spin" size={24} /><p>Loading quiz submissions…</p></div>
      ) : quizzes.length === 0 ? (
        <div className="collection-state"><span className="collection-state-icon"><Check size={23} /></span><h2>No quizzes to review</h2><p>There are no quiz submissions in the system.</p></div>
      ) : (
        <div className="moderation-list">
          {quizzes.map((quiz) => (
            <article className="moderation-card" key={quiz.quizId}>
              {quiz.coverImageId
                ? <img className="moderation-cover" src={imageUrl(quiz.coverImageId)} alt="" />
                : <div className="moderation-cover moderation-cover-empty"><ShieldAlert size={22} /></div>}
              <div className="moderation-content">
                <div className="moderation-topline"><span className="browse-quiz-category">{quiz.category}</span><span className={`quiz-status status-${quiz.status}`}>{statusNames[quiz.status] ?? 'Unknown'}</span></div>
                <h2>{quiz.title}</h2>
                <p>{quiz.description || 'No description provided.'}</p>
                <div className="moderation-meta"><span>By <strong>{quiz.author}</strong></span><span>{quiz.questionCount} questions</span><span>Storage key: <code>{quiz.coverStorageKey ?? '—'}</code></span></div>
                {quiz.status !== 3 && (
                  <div className="moderation-actions">
                    <button type="button" className="button button-primary" disabled={pendingId === quiz.quizId} onClick={() => void moderate(quiz, 3)}><Check size={16} /> Approve</button>
                    <input aria-label={`Reason for blocking ${quiz.title}`} placeholder="Reason required to block" value={reasons[quiz.quizId] ?? ''} onChange={(event) => setReasons((current) => ({ ...current, [quiz.quizId]: event.target.value }))} maxLength={1000} />
                    <button type="button" className="button button-danger" disabled={pendingId === quiz.quizId} onClick={() => void moderate(quiz, 4)}><X size={16} /> Block</button>
                  </div>
                )}
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
