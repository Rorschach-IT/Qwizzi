import { ChartNoAxesColumnIncreasing } from 'lucide-react';

export function AdminStatisticsPage() {
  return (
    <section className="page-container">
      <div className="page-heading">
        <span className="eyebrow">ADMINISTRATION</span>
        <h1>Platform statistics</h1>
        <p>Get a high-level view of activity across Qwizzi.</p>
      </div>
      <div className="collection-state" role="status">
        <div className="collection-state-icon">
          <ChartNoAxesColumnIncreasing size={25} aria-hidden="true" />
        </div>
        <span className="eyebrow">INSIGHTS ARE ON THE WAY</span>
        <h2>No platform statistics yet.</h2>
        <p>Platform-wide activity will appear here when the statistics API is available.</p>
      </div>
    </section>
  );
}
