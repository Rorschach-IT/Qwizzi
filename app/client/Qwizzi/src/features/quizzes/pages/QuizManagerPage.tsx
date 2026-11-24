import { ArrowRight, CirclePlus, PencilLine } from 'lucide-react';
import { Link } from 'react-router-dom';

export function QuizManagerPage() {
  return (
    <section className="page-container">
      <div className="page-heading page-heading-row">
        <div>
          <span className="eyebrow">YOUR CREATIVE CORNER</span>
          <h1>Quiz Manager</h1>
          <p>Create, edit, and share quizzes that make people think.</p>
        </div>
        <button className="button button-primary" type="button" disabled title="Quiz endpoints are not available yet">
          <CirclePlus size={18} aria-hidden="true" /> Create a quiz
        </button>
      </div>
      <div className="manager-welcome">
        <div className="manager-illustration" aria-hidden="true">
          <span><PencilLine size={30} /></span>
          <i /><i /><i />
        </div>
        <div className="manager-copy">
          <span className="eyebrow">YOUR NEXT GREAT QUIZ STARTS HERE</span>
          <h2>Ready to make learning memorable?</h2>
          <p>Quiz creation and editing are on the way. The current backend does not yet expose quiz management endpoints, so your drafts will appear here once they’re connected.</p>
          <Link to="/browse" className="card-link">Explore the library <ArrowRight size={16} aria-hidden="true" /></Link>
        </div>
      </div>
    </section>
  );
}
