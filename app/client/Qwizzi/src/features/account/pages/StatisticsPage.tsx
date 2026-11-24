import { ChartNoAxesColumnIncreasing } from 'lucide-react';

export function StatisticsPage() {
  return (
    <section className="page-container">
      <div className="page-heading">
        <span className="eyebrow">YOUR PROGRESS</span>
        <h1>Statistics</h1>
        <p>A little progress adds up to a lot of learning.</p>
      </div>
      <div className="collection-state" role="status">
        <div className="collection-state-icon"><ChartNoAxesColumnIncreasing size={25} aria-hidden="true" /></div>
        <span className="eyebrow">YOUR JOURNEY STARTS HERE</span>
        <h2>No activity to show yet.</h2>
        <p>Quiz attempt statistics will appear here when the quiz and statistics APIs are available.</p>
      </div>
    </section>
  );
}
