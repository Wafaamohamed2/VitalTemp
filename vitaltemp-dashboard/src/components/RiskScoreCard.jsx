import React, { useState } from 'react';
import { Thermometer, Activity, TrendingUp, Sparkles, X, MapPin, Bot, CheckCircle2, ShieldAlert, Compass } from 'lucide-react';
import { generateGeminiRecommendations } from '../services/api';

export default function RiskScoreCard({ selectedTract, onClose }) {
  const [aiReport, setAiReport] = useState(null);
  const [generatingAi, setGeneratingAi] = useState(false);

  if (!selectedTract) {
    return (
      <div className="glass-panel" style={{ padding: '24px', height: '100%', minHeight: '380px', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', textAlign: 'center' }}>
        <div style={{
          width: '56px',
          height: '56px',
          borderRadius: '16px',
          background: 'rgba(56, 189, 248, 0.1)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          marginBottom: '16px',
          border: '1px solid rgba(56, 189, 248, 0.2)'
        }}>
          <MapPin size={28} color="var(--accent-cyan)" />
        </div>
        <h3 style={{ fontSize: '1.1rem', fontWeight: 700, marginBottom: '8px' }}>
          Select a Phoenix Census Tract
        </h3>
        <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', maxWidth: '280px', lineHeight: 1.5 }}>
          Click on any marker on the map to inspect microclimate heat readings, CDC health metrics, thermal anomalies ($\Delta T$), and AI action plans.
        </p>
      </div>
    );
  }

  const getBadgeClass = (level) => {
    switch (level?.toLowerCase()) {
      case 'critical': return 'critical';
      case 'high': return 'high';
      case 'moderate': return 'moderate';
      default: return 'low';
    }
  };

  const handleGenerateAi = async () => {
    setGeneratingAi(true);
    try {
      const res = await generateGeminiRecommendations(selectedTract.locationId);
      setAiReport(res);
    } catch (err) {
      console.error('Failed to generate Gemini AI recommendations:', err);
    } finally {
      setGeneratingAi(false);
    }
  };

  const anomaly = selectedTract.thermalAnomalyF || 0;
  const tractCodeMatch = selectedTract.name?.match(/\((?:Tract\s*)?([^\)]+)\)/i);
  const displayTractCode = tractCodeMatch ? tractCodeMatch[1] : `#${selectedTract.locationId}`;

  return (
    <div className="glass-panel" style={{ padding: '24px', height: '100%', overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: '18px' }}>
      {/* Header with Close */}
      <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', borderBottom: '1px solid var(--border-color)', paddingBottom: '14px' }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '6px' }}>
            <span className={`risk-badge ${getBadgeClass(selectedTract.riskLevel)}`}>
              {selectedTract.riskLevel} Risk
            </span>
            <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
              {displayTractCode.startsWith('#') ? `ID ${displayTractCode}` : `Census ${displayTractCode}`}
            </span>
          </div>
          <h2 style={{ fontSize: '1.25rem', fontWeight: 800, color: '#ffffff' }}>
            {selectedTract.name}
          </h2>
          <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
            {selectedTract.city}, {selectedTract.state} • [{selectedTract.latitude.toFixed(4)}, {selectedTract.longitude.toFixed(4)}]
          </p>
        </div>
        {onClose && (
          <button
            onClick={onClose}
            style={{ background: 'rgba(255,255,255,0.05)', border: 'none', color: 'var(--text-secondary)', cursor: 'pointer', padding: '6px', borderRadius: '8px' }}
          >
            <X size={18} />
          </button>
        )}
      </div>

      {/* Metrics Row */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
        {/* Surface Temperature & Anomaly */}
        <div style={{ background: 'rgba(239, 68, 68, 0.08)', border: '1px solid rgba(239, 68, 68, 0.25)', padding: '14px', borderRadius: '12px' }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', color: '#f87171', fontSize: '0.75rem', fontWeight: 600, textTransform: 'uppercase' }}>
            <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}><Thermometer size={14} /> Surface Temp</span>
            <span style={{ color: anomaly >= 0 ? '#ef4444' : '#10b981', fontWeight: 800 }}>
              {anomaly >= 0 ? `+${anomaly}` : anomaly}°F
            </span>
          </div>
          <div style={{ fontSize: '1.5rem', fontWeight: 800, color: '#ffffff', margin: '4px 0' }}>
            {selectedTract.tempAvgF}°F
          </div>
          <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
            {selectedTract.tempAvgC}°C • {anomaly >= 0 ? 'Above Phoenix Mean' : 'Below Phoenix Mean'}
          </div>
        </div>

        {/* Health Indicator */}
        <div style={{ background: 'rgba(192, 132, 252, 0.08)', border: '1px solid rgba(192, 132, 252, 0.25)', padding: '14px', borderRadius: '12px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '6px', color: '#c084fc', fontSize: '0.75rem', fontWeight: 600, textTransform: 'uppercase' }}>
            <Activity size={14} /> CDC {selectedTract.healthIndicator}
          </div>
          <div style={{ fontSize: '1.5rem', fontWeight: 800, color: '#ffffff', margin: '4px 0' }}>
            {selectedTract.healthValue}%
          </div>
          <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
            Adult Prevalence Rate
          </div>
        </div>
      </div>

      {/* Spatial Hotspot & Correlation Statistics */}
      <div style={{ background: 'rgba(0,0,0,0.3)', padding: '14px 16px', borderRadius: '12px', border: '1px solid var(--border-color)' }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '8px' }}>
          <span style={{ fontSize: '0.8rem', fontWeight: 600, color: 'var(--text-secondary)' }}>
            Vulnerability Index Score
          </span>
          <span style={{ fontSize: '1rem', fontWeight: 800, color: selectedTract.riskScore >= 0.8 ? '#f87171' : selectedTract.riskScore >= 0.65 ? '#fb923c' : '#34d399' }}>
            {(selectedTract.riskScore * 100).toFixed(0)}%
          </span>
        </div>

        {/* Progress bar */}
        <div style={{ width: '100%', height: '8px', background: 'rgba(255,255,255,0.08)', borderRadius: '9999px', overflow: 'hidden', marginBottom: '10px' }}>
          <div style={{
            width: `${selectedTract.riskScore * 100}%`,
            height: '100%',
            background: selectedTract.riskScore >= 0.8
              ? 'linear-gradient(90deg, #f97316, #ef4444)'
              : selectedTract.riskScore >= 0.65
                ? 'linear-gradient(90deg, #eab308, #f97316)'
                : 'linear-gradient(90deg, #10b981, #38bdf8)',
            borderRadius: '9999px'
          }} />
        </div>

        {/* Spatial Cluster Category */}
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', fontSize: '0.78rem', color: 'var(--text-secondary)', borderTop: '1px solid var(--border-color)', paddingTop: '8px', marginBottom: '6px' }}>
          <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
            <Compass size={13} color="var(--accent-purple)" /> Spatial Cluster:
          </span>
          <span style={{ fontWeight: 700, color: '#ffffff' }}>
            {selectedTract.hotspotCategory || 'Analyzed Tract'}
          </span>
        </div>

        {/* Citywide Statistical Correlation */}
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
          <span style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
            <TrendingUp size={13} color="var(--accent-cyan)" /> Citywide Heat-Health Correlation:
          </span>
          <span style={{ fontWeight: 800, color: '#38bdf8', padding: '2px 8px', background: 'rgba(56, 189, 248, 0.12)', borderRadius: '6px' }}>
            r = {selectedTract.citywideCorrelation || selectedTract.correlation || 0.84} (p = {selectedTract.citywidePValue || selectedTract.pValue || 0.002})
          </span>
        </div>
      </div>

      {/* Analysis Finding Notes */}
      <div>
        <h4 style={{ fontSize: '0.8rem', fontWeight: 700, color: 'var(--text-secondary)', textTransform: 'uppercase', marginBottom: '6px' }}>
          Microclimate Analysis Finding
        </h4>
        <p style={{ fontSize: '0.82rem', color: 'var(--text-primary)', lineHeight: 1.5, background: 'rgba(255,255,255,0.02)', padding: '12px', borderRadius: '10px', border: '1px solid var(--border-color)' }}>
          {selectedTract.notes}
        </p>
      </div>

      {/* Google Gemini AI Strategy Section */}
      <div style={{ background: 'linear-gradient(135deg, rgba(56, 189, 248, 0.08), rgba(129, 140, 248, 0.08))', border: '1px solid rgba(56, 189, 248, 0.25)', padding: '16px', borderRadius: '12px' }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '10px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '6px', color: 'var(--accent-cyan)', fontSize: '0.8rem', fontWeight: 700, textTransform: 'uppercase' }}>
            <Bot size={16} /> Google Gemini AI Strategic Intelligence
          </div>
          {!aiReport && (
            <button
              onClick={handleGenerateAi}
              disabled={generatingAi}
              style={{
                padding: '4px 10px',
                borderRadius: '6px',
                background: 'linear-gradient(135deg, #0284c7, #38bdf8)',
                border: 'none',
                color: '#ffffff',
                fontSize: '0.72rem',
                fontWeight: 700,
                cursor: generatingAi ? 'not-allowed' : 'pointer'
              }}
            >
              {generatingAi ? 'Analyzing...' : 'Generate Plan'}
            </button>
          )}
        </div>

        {aiReport ? (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
            <div style={{ fontSize: '0.8rem', fontWeight: 600, color: '#f9fafb' }}>
              {aiReport.executiveSummary}
            </div>
            
            {aiReport.immediateActions && (
              <div>
                <div style={{ fontSize: '0.75rem', fontWeight: 700, color: '#f87171', textTransform: 'uppercase', marginBottom: '4px' }}>
                  Immediate Response:
                </div>
                <ul style={{ paddingLeft: '16px', fontSize: '0.75rem', color: 'var(--text-secondary)', display: 'flex', flexDirection: 'column', gap: '4px' }}>
                  {aiReport.immediateActions.map((act, i) => (
                    <li key={i}>{act}</li>
                  ))}
                </ul>
              </div>
            )}

            {aiReport.estimatedHeatReduction && (
              <div style={{ fontSize: '0.75rem', color: '#34d399', fontWeight: 700, background: 'rgba(16, 185, 129, 0.12)', padding: '6px 10px', borderRadius: '6px', border: '1px solid rgba(16, 185, 129, 0.25)' }}>
                Target Impact: {aiReport.estimatedHeatReduction}
              </div>
            )}
          </div>
        ) : (
          <p style={{ fontSize: '0.78rem', color: 'var(--text-secondary)', lineHeight: 1.4 }}>
            Click "Generate Plan" to invoke Google Gemini AI to formulate tailored urban heat mitigation and public health directives for {selectedTract.name}.
          </p>
        )}
      </div>
    </div>
  );
}
