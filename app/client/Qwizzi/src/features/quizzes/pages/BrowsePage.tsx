import { useEffect, useState } from 'react';
import { ArrowRight, BookOpen, LoaderCircle, Search } from 'lucide-react';
import { Link } from 'react-router-dom';
import { ApiError } from '../../../core/api/apiError';
import { QuizListState } from '../components/QuizListState';
import { imageUrl, quizzesService, type QuizCategory, type QuizSummary } from '../api/quizzesService';

export function BrowsePage() {
  const [quizzes, setQuizzes] = useState<QuizSummary[]>([]);
  const [categories, setCategories] = useState<QuizCategory[]>([]);
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let active = true;
    void quizzesService.categories()
      .then((result) => { if (active) setCategories(result); })
      .catch(() => { if (active) setError('Unable to load quiz categories.'); });
    return () => { active = false; };
  }, []);

  useEffect(() => {
    let active = true;
    const timer = window.setTimeout(() => {
      void quizzesService.browse(search, categoryId ? Number(categoryId) : undefined)
        .then((result) => { if (active) setQuizzes(result); })
        .catch((requestError: unknown) => {
          if (!active) return;
          setError(requestError instanceof ApiError ? requestError.message : 'Unable to load quizzes.');
        })
        .finally(() => { if (active) setLoading(false); });
    }, 180);
    return () => {
      active = false;
      window.clearTimeout(timer);
    };
  }, [search, categoryId]);

  return (
    <section className="page-container browse-page">
      <div className="page-heading">
        <span className="eyebrow">THE QUIZ LIBRARY</span>
        <h1>Find something fascinating.</h1>
        <p>Browse approved quizzes, discover new topics, and test what you know.</p>
      </div>
      <div className="browse-toolbar">
        <label className="search-field">
          <Search size={18} aria-hidden="true" />
          <span className="sr-only">Search quizzes</span>
          <input type="search" placeholder="Search quizzes" value={search} onChange={(event) => { setError(''); setLoading(true); setSearch(event.target.value); }} />
        </label>
        <label className="filter-select">
          <span className="sr-only">Filter by category</span>
          <select value={categoryId} onChange={(event) => { setError(''); setLoading(true); setCategoryId(event.target.value); }}>
            <option value="">All topics</option>
            {categories.map((category) => <option value={category.categoryId} key={category.categoryId}>{category.name}</option>)}
          </select>
        </label>
      </div>
      {error && <div className="notice error-notice" role="alert">{error}</div>}
      {loading ? (
        <div className="collection-state" role="status"><LoaderCircle className="spin" size={24} /><p>Finding quizzes…</p></div>
      ) : quizzes.length === 0 ? (
        <QuizListState loading={false} hasQuizzes={false} />
      ) : (
        <div className="browse-quiz-grid">
          {quizzes.map((quiz) => (
            <article className="browse-quiz-card" key={quiz.quizId}>
              {quiz.coverImageId
                ? <img className="browse-quiz-cover" src={imageUrl(quiz.coverImageId)} alt="" />
                : <div className="browse-quiz-cover browse-quiz-cover-empty"><BookOpen size={29} aria-hidden="true" /></div>}
              <div className="browse-quiz-body">
                <span className="browse-quiz-category">{quiz.category}</span>
                <h2>{quiz.title}</h2>
                <p>{quiz.description || 'A new quiz to challenge your knowledge.'}</p>
                <div className="browse-quiz-footer"><span>{quiz.questionCount} questions</span><Link to={`/quiz/${quiz.quizId}`} className="card-link">Play quiz <ArrowRight size={16} aria-hidden="true" /></Link></div>
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
