import {
  ArrowRight,
  BookOpenCheck,
  ChartNoAxesColumnIncreasing,
  CircleHelp,
  Sparkles,
} from 'lucide-react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../auth/context/useAuth';

const highlights = [
  {
    icon: BookOpenCheck,
    title: 'Find your next quiz',
    text: 'Explore a growing collection of quizzes and learn at your own pace.',
    to: '/browse',
    action: 'Explore quizzes',
    tone: 'mint',
  },
  {
    icon: Sparkles,
    title: 'Make it your own',
    text: 'Build quizzes around the topics you care about and share them with others.',
    to: '/quiz-manager',
    action: 'Open Quiz Manager',
    tone: 'peach',
  },
  {
    icon: ChartNoAxesColumnIncreasing,
    title: 'See how far you’ve come',
    text: 'Your learning journey is easier to follow when your progress is all in one place.',
    to: '/account/statistics',
    action: 'View statistics',
    tone: 'lavender',
  },
];

export function HomePage() {
  const { user, error } = useAuth();

  return (
    <div className="home-page">
      {error && <div className="service-banner" role="status">{error} You can still explore Qwizzi as a guest.</div>}
      <section className="hero-section">
        <div className="hero-copy">
          <span className="eyebrow hero-eyebrow"><Sparkles size={14} /> LEARNING, WITH A LITTLE MORE PLAY</span>
          <h1>{user ? `Good to see you, ${user.username}.` : 'Curiosity looks good on you.'}</h1>
          <p>Discover quizzes, challenge yourself, and turn the things you love into something you know.</p>
          <div className="hero-actions">
            <Link to="/browse" className="button button-accent">
              Explore quizzes <ArrowRight size={17} aria-hidden="true" />
            </Link>
            <Link to="/quiz-manager" className="button button-quiet">
              Create a quiz
            </Link>
          </div>
          <div className="hero-note">
            <span className="hero-note-icon"><CircleHelp size={17} /></span>
            <span>Pick a topic. Take a quiz. Keep getting better.</span>
          </div>
        </div>
        <div className="hero-art" aria-hidden="true">
          <div className="art-sun" />
          <div className="art-card art-card-main">
            <div className="art-card-top"><span>YOUR NEXT BIG IDEA</span><Sparkles size={16} /></div>
            <div className="art-question">What will you<br />learn today?</div>
            <div className="art-options"><span /><span /><span /></div>
            <div className="art-progress"><span /></div>
          </div>
          <div className="art-floating art-badge"><BookOpenCheck size={19} /><span>Ready, set, learn</span></div>
          <div className="art-floating art-star">✳</div>
          <div className="art-dots" />
        </div>
      </section>

      <section className="intro-section">
        <div>
          <span className="eyebrow">YOUR SPACE TO GROW</span>
          <h2>Small quizzes. Big “aha!” moments.</h2>
        </div>
        <p>Whether you’re here to explore or make something of your own, Qwizzi gives every curious mind a place to start.</p>
      </section>

      <section className="highlight-grid" aria-label="What you can do with Qwizzi">
        {highlights.map(({ icon: Icon, ...item }, index) => (
          <article className={`highlight-card highlight-${item.tone}`} key={item.title}>
            <div className="highlight-top">
              <span className="highlight-icon"><Icon size={20} aria-hidden="true" /></span>
              <span className="highlight-index">0{index + 1}</span>
            </div>
            <h3>{item.title}</h3>
            <p>{item.text}</p>
            <Link to={item.to} className="card-link">
              {item.action} <ArrowRight size={16} aria-hidden="true" />
            </Link>
          </article>
        ))}
      </section>

      {!user && (
        <section className="signup-banner">
          <div>
            <span className="eyebrow">A LITTLE MORE QWIZZI</span>
            <h2>Make your curiosity count.</h2>
            <p>Create an account to build quizzes and keep track of your progress.</p>
          </div>
          <Link className="button button-light" to="/register">
            Join Qwizzi <ArrowRight size={17} aria-hidden="true" />
          </Link>
        </section>
      )}
    </div>
  );
}
