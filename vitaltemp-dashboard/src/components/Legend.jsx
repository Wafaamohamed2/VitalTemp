import React from 'react';
import { Info } from 'lucide-react';

export default function Legend() {
  const items = [
    { level: "Critical Risk", range: "Score ≥ 0.80", temp: "> 112°F", color: "#ef4444" },
    { level: "High Risk", range: "0.65 – 0.79", temp: "108°F – 111°F", color: "#f97316" },
    { level: "Moderate Risk", range: "0.45 – 0.64", temp: "104°F – 107°F", color: "#eab308" },
    { level: "Low Risk", range: "< 0.45", temp: "< 103°F", color: "#10b981" }
  ];

  return (
    <div className="glass-panel" style={{ padding: '16px 20px', marginTop: '20px' }}>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '12px' }}>
        <h4 style={{ fontSize: '0.85rem', fontWeight: 700, color: 'var(--text-secondary)', textTransform: 'uppercase', letterSpacing: '0.05em', display: 'flex', alignItems: 'center', gap: '6px' }}>
          <Info size={14} color="var(--accent-cyan)" /> Risk Score & Heat Gradient Legend
        </h4>
        <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
          Normalized Index (0.0 to 1.0)
        </span>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '12px' }}>
        {items.map((item, i) => (
          <div key={i} style={{ display: 'flex', alignItems: 'center', gap: '10px', background: 'rgba(255, 255, 255, 0.02)', padding: '8px 12px', borderRadius: '8px', border: '1px solid var(--border-color)' }}>
            <span style={{
              width: '12px',
              height: '12px',
              borderRadius: '50%',
              backgroundColor: item.color,
              boxShadow: `0 0 8px ${item.color}80`,
              flexShrink: 0
            }} />
            <div>
              <div style={{ fontSize: '0.8rem', fontWeight: 700, color: item.color }}>
                {item.level}
              </div>
              <div style={{ fontSize: '0.7rem', color: 'var(--text-secondary)' }}>
                {item.range} • {item.temp}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
