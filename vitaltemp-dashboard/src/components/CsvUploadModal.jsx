import React, { useState } from 'react';
import { UploadCloud, FileText, CheckCircle2, AlertCircle, X } from 'lucide-react';
import { uploadLocationsCsv, uploadHealthCsv } from '../services/api';

export default function CsvUploadModal({ isOpen, onClose, onImportSuccess }) {
  const [locationsFile, setLocationsFile] = useState(null);
  const [healthFile, setHealthFile] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [statusMessage, setStatusMessage] = useState(null);
  const [isError, setIsError] = useState(false);

  if (!isOpen) return null;

  const handleUploadLocations = async (e) => {
    e.preventDefault();
    if (!locationsFile) return;

    setUploading(true);
    setStatusMessage(null);
    try {
      const res = await uploadLocationsCsv(locationsFile);
      setIsError(false);
      setStatusMessage(res.message || `Successfully imported ${res.locationsImported || 0} Phoenix tracts.`);
      onImportSuccess();
    } catch (err) {
      setIsError(true);
      setStatusMessage(err.response?.data?.message || err.message || 'Failed to upload locations CSV.');
    } finally {
      setUploading(false);
    }
  };

  const handleUploadHealth = async (e) => {
    e.preventDefault();
    if (!healthFile) return;

    setUploading(true);
    setStatusMessage(null);
    try {
      const res = await uploadHealthCsv(healthFile);
      setIsError(false);
      setStatusMessage(res.message || `Successfully imported ${res.healthRecordsImported || 0} CDC health records.`);
      onImportSuccess();
    } catch (err) {
      setIsError(true);
      setStatusMessage(err.response?.data?.message || err.message || 'Failed to upload CDC health CSV.');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div style={{
      position: 'fixed',
      inset: 0,
      background: 'rgba(0, 0, 0, 0.75)',
      backdropFilter: 'blur(8px)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      zIndex: 9999,
      padding: '20px'
    }}>
      <div className="glass-panel" style={{
        width: '100%',
        maxWidth: '560px',
        padding: '28px',
        background: '#111827',
        border: '1px solid rgba(255, 255, 255, 0.15)',
        boxShadow: '0 20px 50px rgba(0, 0, 0, 0.8)'
      }}>
        {/* Header */}
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '20px', borderBottom: '1px solid var(--border-color)', paddingBottom: '14px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <div style={{ width: '38px', height: '38px', borderRadius: '10px', background: 'rgba(56, 189, 248, 0.15)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <UploadCloud size={22} color="var(--accent-cyan)" />
            </div>
            <div>
              <h3 style={{ fontSize: '1.2rem', fontWeight: 800 }}>Import Data via CSV</h3>
              <p style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
                Upload Phoenix Census Tracts or CDC PLACES health datasets
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            style={{ background: 'transparent', border: 'none', color: 'var(--text-secondary)', cursor: 'pointer', padding: '4px' }}
          >
            <X size={20} />
          </button>
        </div>

        {/* Status Message */}
        {statusMessage && (
          <div style={{
            padding: '12px 14px',
            borderRadius: '10px',
            marginBottom: '18px',
            background: isError ? 'rgba(239, 68, 68, 0.15)' : 'rgba(16, 185, 129, 0.15)',
            border: `1px solid ${isError ? 'rgba(239, 68, 68, 0.4)' : 'rgba(16, 185, 129, 0.4)'}`,
            color: isError ? '#f87171' : '#34d399',
            fontSize: '0.85rem',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}>
            {isError ? <AlertCircle size={18} /> : <CheckCircle2 size={18} />}
            {statusMessage}
          </div>
        )}

        <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          {/* Section 1: Locations CSV */}
          <div style={{ background: 'rgba(255, 255, 255, 0.02)', border: '1px solid var(--border-color)', borderRadius: '12px', padding: '16px' }}>
            <h4 style={{ fontSize: '0.9rem', fontWeight: 700, marginBottom: '6px', color: 'var(--accent-cyan)' }}>
              1. Phoenix Census Tracts CSV
            </h4>
            <p style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginBottom: '12px' }}>
              Required headers: <code>name</code> (or <code>tract</code>), <code>latitude</code>, <code>longitude</code>
            </p>
            <form onSubmit={handleUploadLocations} style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
              <input
                type="file"
                accept=".csv"
                onChange={(e) => setLocationsFile(e.target.files[0])}
                style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', flex: 1 }}
              />
              <button
                type="submit"
                disabled={!locationsFile || uploading}
                style={{
                  padding: '7px 14px',
                  borderRadius: '8px',
                  background: 'linear-gradient(135deg, #0284c7, #38bdf8)',
                  border: 'none',
                  color: '#ffffff',
                  fontSize: '0.8rem',
                  fontWeight: 700,
                  cursor: (!locationsFile || uploading) ? 'not-allowed' : 'pointer',
                  opacity: (!locationsFile || uploading) ? 0.6 : 1
                }}
              >
                {uploading ? 'Processing...' : 'Upload Tracts'}
              </button>
            </form>
          </div>

          {/* Section 2: CDC PLACES Health CSV */}
          <div style={{ background: 'rgba(255, 255, 255, 0.02)', border: '1px solid var(--border-color)', borderRadius: '12px', padding: '16px' }}>
            <h4 style={{ fontSize: '0.9rem', fontWeight: 700, marginBottom: '6px', color: 'var(--accent-purple)' }}>
              2. CDC PLACES Health Measures CSV
            </h4>
            <p style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginBottom: '12px' }}>
              Required headers: <code>LocationName</code> (or <code>tract</code>), <code>Indicator</code>, <code>Value</code>
            </p>
            <form onSubmit={handleUploadHealth} style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
              <input
                type="file"
                accept=".csv"
                onChange={(e) => setHealthFile(e.target.files[0])}
                style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', flex: 1 }}
              />
              <button
                type="submit"
                disabled={!healthFile || uploading}
                style={{
                  padding: '7px 14px',
                  borderRadius: '8px',
                  background: 'linear-gradient(135deg, #7c3aed, #c084fc)',
                  border: 'none',
                  color: '#ffffff',
                  fontSize: '0.8rem',
                  fontWeight: 700,
                  cursor: (!healthFile || uploading) ? 'not-allowed' : 'pointer',
                  opacity: (!healthFile || uploading) ? 0.6 : 1
                }}
              >
                {uploading ? 'Processing...' : 'Upload Health'}
              </button>
            </form>
          </div>
        </div>

        {/* Close Button */}
        <div style={{ marginTop: '20px', display: 'flex', justifyContent: 'flex-end' }}>
          <button
            onClick={onClose}
            style={{
              padding: '8px 18px',
              borderRadius: '8px',
              background: 'rgba(255, 255, 255, 0.08)',
              border: '1px solid var(--border-color)',
              color: 'var(--text-primary)',
              fontSize: '0.85rem',
              fontWeight: 600,
              cursor: 'pointer'
            }}
          >
            Done
          </button>
        </div>
      </div>
    </div>
  );
}
