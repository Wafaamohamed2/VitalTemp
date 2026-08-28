import React, { useState, useEffect } from 'react';
import Header from './components/Header';
import SummaryCards from './components/SummaryCards';
import FilterBar from './components/FilterBar';
import MapView from './components/MapView';
import RiskScoreCard from './components/RiskScoreCard';
import Legend from './components/Legend';
import CsvUploadModal from './components/CsvUploadModal';
import { getNeighborhoodRiskScores, getDashboardSummary, syncFortyGuardHeat } from './services/api';
import { Loader2, SearchX } from 'lucide-react';

export default function App() {
  const [neighborhoods, setNeighborhoods] = useState([]);
  const [summary, setSummary] = useState(null);
  const [selectedTract, setSelectedTract] = useState(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedLevel, setSelectedLevel] = useState('All');
  const [selectedIndicator, setSelectedIndicator] = useState('ALL');
  const [mode, setMode] = useState('combined'); // 'combined' | 'thermal' | 'health'
  const [isLive, setIsLive] = useState(true);
  const [loading, setLoading] = useState(true);
  const [syncing, setSyncing] = useState(false);
  const [isCsvModalOpen, setIsCsvModalOpen] = useState(false);
  const [toast, setToast] = useState(null);

  const showToast = (message, type = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 4500);
  };

  const fetchData = async () => {
    setLoading(true);
    try {
      const [scoresRes, summaryRes] = await Promise.all([
        getNeighborhoodRiskScores(selectedIndicator),
        getDashboardSummary(selectedIndicator)
      ]);

      const data = scoresRes.data || [];
      setNeighborhoods(data);
      setIsLive(scoresRes.isLive);
      setSummary(summaryRes);

      // Keep selected tract synchronized with fresh calculations
      setSelectedTract(prev => {
        if (!prev) return data[0] || null;
        const matched = data.find(n => n.locationId === prev.locationId);
        return matched || data[0] || null;
      });
    } catch (err) {
      console.error('Failed to load data:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSyncFortyGuard = async () => {
    setSyncing(true);
    try {
      const res = await syncFortyGuardHeat();
      showToast(res.message || 'FortyGuard thermal heatmap synced and correlation recalculated!', 'success');
      await fetchData();
    } catch (err) {
      console.warn('FortyGuard sync failed:', err);
      showToast('Could not reach FortyGuard live server. Active session fallback engaged.', 'warning');
      await fetchData();
    } finally {
      setSyncing(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [selectedIndicator]);

  // Filtered Neighborhoods
  const filteredNeighborhoods = neighborhoods.filter(tract => {
    const matchesSearch = tract.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
                          tract.city.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesLevel = selectedLevel === 'All' || tract.riskLevel.toLowerCase() === selectedLevel.toLowerCase();
    return matchesSearch && matchesLevel;
  });

  const getToastBackground = (type) => {
    switch (type) {
      case 'error': return 'rgba(239, 68, 68, 0.95)';
      case 'warning': return 'rgba(245, 158, 11, 0.95)';
      default: return 'rgba(16, 185, 129, 0.95)';
    }
  };

  return (
    <div style={{ maxWidth: '1440px', margin: '0 auto', padding: '20px 16px', minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      {/* Toast Notification */}
      {toast && (
        <div style={{
          position: 'fixed',
          top: '24px',
          right: '24px',
          padding: '12px 20px',
          borderRadius: '10px',
          background: getToastBackground(toast.type),
          color: '#ffffff',
          fontWeight: 700,
          fontSize: '0.85rem',
          boxShadow: '0 10px 30px rgba(0,0,0,0.5)',
          zIndex: 10000,
          backdropFilter: 'blur(8px)'
        }}>
          {toast.message}
        </div>
      )}

      {/* Top Header */}
      <Header
        isLive={isLive}
        onRefresh={fetchData}
        onSyncFortyGuard={handleSyncFortyGuard}
        onOpenCsvModal={() => setIsCsvModalOpen(true)}
        loading={loading}
        syncing={syncing}
      />

      {/* KPI Stats Bar */}
      <SummaryCards neighborhoods={neighborhoods} summary={summary} />

      {/* Filter and Multi-Indicator Control Bar */}
      <FilterBar
        searchQuery={searchQuery}
        setSearchQuery={setSearchQuery}
        selectedLevel={selectedLevel}
        setSelectedLevel={setSelectedLevel}
        mode={mode}
        setMode={setMode}
        selectedIndicator={selectedIndicator}
        setSelectedIndicator={setSelectedIndicator}
        totalCount={neighborhoods.length}
        filteredCount={filteredNeighborhoods.length}
      />

      {/* Loading State Overlay / Skeleton */}
      {loading && (
        <div style={{ padding: '20px', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '10px', color: 'var(--accent-cyan)', fontSize: '0.9rem', fontWeight: 600 }}>
          <Loader2 className="pulse-circle" size={20} /> Loading Phoenix spatial heat models & health indices...
        </div>
      )}

      {/* Main Visual Intelligence Grid */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(360px, 1fr))',
        gap: '20px',
        flex: '1',
        alignItems: 'stretch'
      }}>
        {/* Left Column: Map */}
        <div style={{ flex: '1.6', minWidth: '340px' }}>
          {filteredNeighborhoods.length === 0 && !loading ? (
            <div className="glass-panel" style={{ height: '420px', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', textAlign: 'center', padding: '24px' }}>
              <SearchX size={48} color="var(--text-muted)" style={{ marginBottom: '12px' }} />
              <h3 style={{ fontSize: '1.1rem', fontWeight: 700 }}>No Census Tracts Match Filters</h3>
              <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '4px' }}>
                Try adjusting your search keyword or selected risk level.
              </p>
            </div>
          ) : (
            <MapView
              neighborhoods={filteredNeighborhoods}
              selectedTract={selectedTract}
              onSelectTract={setSelectedTract}
              mode={mode}
            />
          )}

          {/* Legend */}
          <Legend />
        </div>

        {/* Right Column: Deep-Dive Intelligence Panel */}
        <div style={{ flex: '1', minWidth: '320px' }}>
          <RiskScoreCard
            selectedTract={selectedTract}
            selectedIndicator={selectedIndicator}
            onClose={() => setSelectedTract(null)}
          />
        </div>
      </div>

      {/* CSV Import Modal */}
      <CsvUploadModal
        isOpen={isCsvModalOpen}
        onClose={() => setIsCsvModalOpen(false)}
        onImportSuccess={() => {
          showToast('CSV data imported successfully! Recalculating analysis...', 'success');
          fetchData();
        }}
      />

      {/* Footer */}
      <footer style={{ marginTop: '28px', padding: '16px 0', borderTop: '1px solid var(--border-color)', display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: '12px', color: 'var(--text-muted)', fontSize: '0.8rem' }}>
        <div>
          VitalTemp • Team 610 (Wafaa & Hamza) • FortyGuard Hackathon'26
        </div>
        <div style={{ display: 'flex', gap: '16px' }}>
          <span>Architecture: ASP.NET Core 8 Web API + SQLite + React + Leaflet</span>
          <span>Target: Phoenix, AZ</span>
        </div>
      </footer>
    </div>
  );
}
