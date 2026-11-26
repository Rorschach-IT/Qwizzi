import { useEffect, useState } from 'react';
import { ArrowLeft, ArrowRight, CheckCircle2, LoaderCircle, RotateCcw, Trophy, XCircle } from 'lucide-react';
import { Link, useParams } from 'react-router-dom';
import { quizzesService, imageUrl, type QuizAttemptResult, type QuizStart } from '../../quizzes/api/quizzesService';

export function QuizPlayPage() {
  const { quizId = '' } = useParams();
  const [session, setSession] = useState<QuizStart | null>(null);
  const [answers, setAnswers] = useState<Record<number, number>>({});
  const [result, setResult] = useState<QuizAttemptResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    let active = true;
    void quizzesService.start(quizId)
      .then((started) => { if (active) setSession(started); })
      .catch((requestError: unknown) => {
        if (active) setError(requestError instanceof Error ? requestError.message : 'This quiz is unavailable.');
      })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [quizId]);

  async function submit() {
    if (!session) return;
    setSubmitting(true);
    setError('');
    try {
      setResult(await quizzesService.submit(session.attemptId, answers));
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Unable to submit your answers.');
    } finally {
      setSubmitting(false);
    }
  }

  if (loading) {
    return <section className="page-container"><div className="collection-state" role="status"><LoaderCircle className="spin" size={24} /><p>Opening quiz…</p></div></section>;
  }
  if (error && !session) {
    return <section className="page-container"><div className="collection-state"><span className="collection-state-icon"><XCircle size={24} /></span><h2>Quiz unavailable</h2><p>{error}</p><Link className="button button-outline" to="/browse">Back to quizzes</Link></div></section>;
  }
  if (!session) return null;

  const { quiz } = session;
  const answeredCount = Object.keys(answers).length;

  return (
    <section className="page-container quiz-play-page">
      <Link to="/browse" className="quiz-back-link"><ArrowLeft size={16} /> All quizzes</Link>
      <div className="quiz-play-heading">
        <span className="eyebrow">{quiz.category}</span>
        <h1>{quiz.title}</h1>
        <p>{result ? 'Your results are ready.' : `${answeredCount} of ${quiz.questions.length} questions answered`}</p>
        <div className="quiz-play-progress"><span style={{ width: `${result ? 100 : answeredCount / quiz.questions.length * 100}%` }} /></div>
      </div>

      {error && <div className="notice error-notice" role="alert">{error}</div>}
      {result ? (
        <section className="quiz-result-card" aria-labelledby="quiz-result-title">
          <div className="quiz-result-icon"><Trophy size={25} aria-hidden="true" /></div>
          <span className="eyebrow">QUIZ COMPLETE</span>
          <h2 id="quiz-result-title">{result.score} out of {result.totalQuestions} correct</h2>
          <p>You scored {Math.round(result.score / Math.max(1, result.totalQuestions) * 100)}%.</p>
          <div className="quiz-result-review">
            {quiz.questions.map((question) => {
              const answer = result.questions.find((item) => item.questionId === question.questionId);
              return (
                <article className={`quiz-review-question${answer?.isCorrect ? ' review-correct' : ' review-incorrect'}`} key={question.questionId}>
                  <div className="quiz-review-status">{answer?.isCorrect ? <CheckCircle2 size={18} /> : <XCircle size={18} />}</div>
                  <div>
                    <h3>{question.text}</h3>
                    <p>Your answer: {question.answers.find((item) => item.answerId === answer?.selectedAnswerId)?.text ?? 'Image answer'}</p>
                    {!answer?.isCorrect && <p>Correct answer: {question.answers.find((item) => item.answerId === answer?.correctAnswerId)?.text ?? 'Image answer'}</p>}
                  </div>
                </article>
              );
            })}
          </div>
          <div className="quiz-result-actions"><Link className="button button-outline" to="/browse">Back to quizzes</Link><Link className="button button-primary" to="/account/statistics">View statistics <ArrowRight size={16} /></Link></div>
        </section>
      ) : (
        <>
          <div className="quiz-play-questions">
            {quiz.questions.map((question, index) => (
              <fieldset className="quiz-play-question" key={question.questionId}>
                <legend><span>Question {index + 1} of {quiz.questions.length}</span><strong>{question.text}</strong></legend>
                {question.imageId && <img className="quiz-play-question-image" src={imageUrl(question.imageId)} alt="" />}
                <div className="quiz-play-options">
                  {question.answers.map((answer) => (
                    <label className={`quiz-play-option${answers[question.questionId] === answer.answerId ? ' selected' : ''}`} key={answer.answerId}>
                      <input type="radio" name={`question-${question.questionId}`} checked={answers[question.questionId] === answer.answerId} onChange={() => setAnswers((current) => ({ ...current, [question.questionId]: answer.answerId }))} />
                      {answer.imageId
                        ? <img src={imageUrl(answer.imageId)} alt={`Answer option`} />
                        : <span>{answer.text}</span>}
                    </label>
                  ))}
                </div>
              </fieldset>
            ))}
          </div>
          <div className="quiz-submit-row">
            <p>{answeredCount === quiz.questions.length ? 'Ready to see how you did?' : 'Answer every question to finish.'}</p>
            <button type="button" className="button button-primary" onClick={() => void submit()} disabled={answeredCount !== quiz.questions.length || submitting}>
              {submitting ? <LoaderCircle className="spin" size={17} /> : <CheckCircle2 size={17} />}
              {submitting ? 'Submitting…' : 'Finish quiz'}
            </button>
          </div>
        </>
      )}
      {!result && <p className="quiz-play-reset"><button type="button" className="text-button" onClick={() => { setAnswers({}); setError(''); }}><RotateCcw size={14} /> Clear my answers</button></p>}
    </section>
  );
}
