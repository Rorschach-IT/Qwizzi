import { Search } from 'lucide-react';
import { QuizListState } from '../components/QuizListState';

export function BrowsePage() {
  return (
    <section className="page-container">
      <div className="page-heading">
        <span className="eyebrow">THE QUIZ LIBRARY</span>
        <h1>Find something fascinating.</h1>
        <p>Browse quizzes, discover new topics, and test what you know.</p>
      </div>
      <div className="browse-toolbar">
        <label className="search-field">
          <Search size={18} aria-hidden="true" />
          <span className="sr-only">Search quizzes</span>
          <input type="search" placeholder="Search quizzes" disabled aria-describedby="browse-search-note" />
        </label>
        <span className="filter-pill">All topics</span>
      </div>
      <div id="browse-search-note">
        <QuizListState loading={false} hasQuizzes={false} />
      </div>
    </section>
  );
}
