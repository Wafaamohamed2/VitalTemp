import React from 'react';
import { Flame, ShieldAlert, Activity, RefreshCw, UploadCloud, Zap } from 'lucide-react';

export default function Header({ isLive, onRefresh, onSyncFortyGuard, onOpenCsvModal, loading, syncing }) {
  return (
    <header className="glass-panel" style={{ padding: '16px 24px', marginBottom: '20px', display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: '16px' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '14px' }}>
        <div style={{
          width: '46px',
          height: '46px',
          borderRadius: '12px',
          background: 'linear-gradient(135deg, #ef4444 0%, #f97316 100%)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          boxShadow: '0 0 20px rgba(239, 68, 68, 0.4)'
        }}>
          <Flame size={28} color="#ffffff" />
        </div>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <h1 style={{ fontSize: '1.6rem', fontWeight: 800, letterSpacing: '-0.02em', background: 'linear-gradient(90deg, #ffffff, #93c5fd)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent' }}>
              VitalTemp
            </h1>
            <span style={{ fontSize: '0.75rem', padding: '2px 8px', borderRadius: '6px', background: 'rgba(56, 189, 248, 0.15)', color: '#38bdf8', border: '1px solid rgba(56, 189, 248, 0.3)', fontWeight: 600 }}>
              Team 610
            </span>
          </div>
          <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '2px' }}>
            Phoenix Urban Heat Island & Health Vulnerability Intelligence • FortyGuard Hackathon'26
          </p>
        </div>
      </div>

      <div style={{ display: 'flex', alignItems: 'center', gap: '10px', flexWrap: 'wrap' }}>
        {/* Live Status Badge */}
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '8px',
          padding: '6px 14px',
          borderRadius: '9999px',
          background: isLive ? 'rgba(16, 185, 129, 0.12)' : 'rgba(234, 179, 8, 0.12)',
          border: isLive ? '1px solid rgba(16, 185, 129, 0.3)' : '1px solid rgba(234, 179, 8, 0.3)',
          fontSize: '0.8rem',
          fontWeight: 600,
          color: isLive ? '#34d399' : '#facc15'
        }}>
          <span style={{
            width: '8px',
            height: '8px',
            borderRadius: '50%',
            backgroundColor: isLive ? '#10b981' : '#eab308',
            boxShadow: isLive ? '0 0 8px #10b981' : '0 0 8px #eab308'
          }} />
          {isLive ? 'Live' : 'Cached'}
        </div>

        {/* Sync FortyGuard Heat Button */}
        <button
          onClick={onSyncFortyGuard}
          disabled={syncing}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            padding: '8px 14px',
            borderRadius: '10px',
            background: 'linear-gradient(135deg, rgba(239, 68, 68, 0.2), rgba(249, 115, 22, 0.2))',
            border: '1px solid rgba(239, 68, 68, 0.4)',
            color: '#fb923c',
            fontSize: '0.82rem',
            fontWeight: 700,
            cursor: syncing ? 'not-allowed' : 'pointer',
            transition: 'all 0.2s ease'
          }}
          onMouseEnter={(e) => e.currentTarget.style.borderColor = '#ef4444'}
          onMouseLeave={(e) => e.currentTarget.style.borderColor = 'rgba(239, 68, 68, 0.4)'}
        >
          <Zap size={14} className={syncing ? 'pulse-circle' : ''} />
          {syncing ? 'Syncing...' : 'Sync FortyGuard'}
        </button>

        {/* Import CSV Modal Trigger */}
        <button
          onClick={onOpenCsvModal}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            padding: '8px 14px',
            borderRadius: '10px',
            background: 'rgba(56, 189, 248, 0.12)',
            border: '1px solid rgba(56, 189, 248, 0.35)',
            color: 'var(--accent-cyan)',
            fontSize: '0.82rem',
            fontWeight: 700,
            cursor: 'pointer',
            transition: 'all 0.2s ease'
          }}
          onMouseEnter={(e) => e.currentTarget.style.background = 'rgba(56, 189, 248, 0.2)'}
          onMouseLeave={(e) => e.currentTarget.style.background = 'rgba(56, 189, 248, 0.12)'}
        >
          <UploadCloud size={15} />
          Import CSV
        </button>

        {/* Refresh Button */}
        <button
          onClick={onRefresh}
          disabled={loading}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            padding: '8px 14px',
            borderRadius: '10px',
            background: 'rgba(255, 255, 255, 0.06)',
            border: '1px solid var(--border-color)',
            color: 'var(--text-primary)',
            fontSize: '0.82rem',
            fontWeight: 600,
            cursor: loading ? 'not-allowed' : 'pointer',
            transition: 'all 0.2s ease'
          }}
        >
          <RefreshCw size={14} className={loading ? 'pulse-circle' : ''} />
        </button>
      </div>
    </header>
  );
}
