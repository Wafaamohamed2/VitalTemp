import React from 'react';
import { Search, Filter, Activity, Layers, MapPin } from 'lucide-react';

export default function FilterBar({
  searchQuery,
  setSearchQuery,
  selectedLevel,
  setSelectedLevel,
  mode,
  setMode,
  selectedIndicator,
  setSelectedIndicator,
  totalCount,
  filteredCount
}) {
  const levels = ['All', 'Critical', 'High', 'Moderate', 'Low'];

  const indicators = [
    { key: 'ALL', label: 'All Measures (Composite)' },
    { key: 'ASTHMA', label: 'Asthma Prevalence (CDC)' },
    { key: 'BPHIGH', label: 'High Blood Pressure (CDC)' },
    { key: 'CHD', label: 'Coronary Heart Disease (CDC)' },
    { key: 'DIABETES', label: 'Diabetes Prevalence (CDC)' },
  ];

  return (
    <div className="glass-panel" style={{ padding: '14px 20px', marginBottom: '20px', display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: '14px' }}>
      {/* Search Input */}
      <div style={{ position: 'relative', flex: '1 1 200px', minWidth: '180px', maxWidth: '320px' }}>
        <Search size={16} color="var(--text-muted)" style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)' }} />
        <input
          type="text"
          placeholder="Search tract (e.g. Maryvale, Downtown)..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          style={{
            width: '100%',
            padding: '8px 12px 8px 36px',
            borderRadius: '10px',
            background: 'rgba(0, 0, 0, 0.35)',
            border: '1px solid var(--border-color)',
            color: 'var(--text-primary)',
            fontSize: '0.85rem',
            outline: 'none',
            fontFamily: 'inherit'
          }}
        />
      </div>

      {/* Health Indicator Dropdown Selector */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '6px', flex: '0 1 auto' }}>
        <Activity size={15} color="var(--accent-purple)" />
        <select
          value={selectedIndicator}
          onChange={(e) => setSelectedIndicator(e.target.value)}
          style={{
            padding: '7px 12px',
            borderRadius: '8px',
            background: 'rgba(192, 132, 252, 0.1)',
            border: '1px solid rgba(192, 132, 252, 0.3)',
            color: '#e9d5ff',
            fontSize: '0.82rem',
            fontWeight: 600,
            outline: 'none',
            cursor: 'pointer',
            fontFamily: 'inherit'
          }}
        >
          {indicators.map(ind => (
            <option key={ind.key} value={ind.key} style={{ background: '#111827', color: '#ffffff' }}>
              {ind.label}
            </option>
          ))}
        </select>
      </div>

      {/* Risk Filter Buttons */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '6px', flexWrap: 'wrap' }}>
        <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginRight: '2px', display: 'flex', alignItems: 'center', gap: '4px' }}>
          <Filter size={14} /> Risk:
        </span>
        {levels.map(level => {
          const isActive = selectedLevel === level;
          return (
            <button
              key={level}
              onClick={() => setSelectedLevel(level)}
              style={{
                padding: '5px 11px',
                borderRadius: '8px',
                fontSize: '0.78rem',
                fontWeight: 600,
                cursor: 'pointer',
                border: '1px solid',
                borderColor: isActive ? 'var(--accent-cyan)' : 'var(--border-color)',
                background: isActive ? 'rgba(56, 189, 248, 0.15)' : 'rgba(255, 255, 255, 0.03)',
                color: isActive ? '#38bdf8' : 'var(--text-secondary)',
                transition: 'all 0.15s ease'
              }}
            >
              {level}
            </button>
          );
        })}
      </div>

      {/* Tract Count Indicator Badge */}
      <div style={{
        display: 'flex',
        alignItems: 'center',
        gap: '6px',
        padding: '5px 10px',
        borderRadius: '8px',
        background: 'rgba(255, 255, 255, 0.04)',
        border: '1px solid var(--border-color)',
        fontSize: '0.75rem',
        color: filteredCount < totalCount ? 'var(--accent-cyan)' : 'var(--text-muted)',
        fontWeight: 600
      }}>
        <MapPin size={13} color="var(--accent-cyan)" />
        <span>Showing {filteredCount} of {totalCount} tracts</span>
      </div>

      {/* Layer Mode Toggle */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '4px', background: 'rgba(0,0,0,0.3)', padding: '4px', borderRadius: '10px', border: '1px solid var(--border-color)' }}>
        <button
          onClick={() => setMode('combined')}
          style={{
            padding: '4px 10px',
            borderRadius: '6px',
            fontSize: '0.75rem',
            fontWeight: 600,
            cursor: 'pointer',
            border: 'none',
            background: mode === 'combined' ? 'rgba(255, 255, 255, 0.12)' : 'transparent',
            color: mode === 'combined' ? '#ffffff' : 'var(--text-muted)'
          }}
        >
          Combined
        </button>
        <button
          onClick={() => setMode('thermal')}
          style={{
            padding: '4px 10px',
            borderRadius: '6px',
            fontSize: '0.75rem',
            fontWeight: 600,
            cursor: 'pointer',
            border: 'none',
            background: mode === 'thermal' ? 'rgba(239, 68, 68, 0.2)' : 'transparent',
            color: mode === 'thermal' ? '#f87171' : 'var(--text-muted)'
          }}
        >
          Thermal
        </button>
        <button
          onClick={() => setMode('health')}
          style={{
            padding: '4px 10px',
            borderRadius: '6px',
            fontSize: '0.75rem',
            fontWeight: 600,
            cursor: 'pointer',
            border: 'none',
            background: mode === 'health' ? 'rgba(192, 132, 252, 0.2)' : 'transparent',
            color: mode === 'health' ? '#c084fc' : 'var(--text-muted)'
          }}
        >
          Health
        </button>
      </div>
    </div>
  );
}
