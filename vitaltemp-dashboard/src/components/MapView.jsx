import React, { useEffect } from 'react';
import { MapContainer, TileLayer, Marker, Popup, Circle, Tooltip, useMap } from 'react-leaflet';
import L from 'leaflet';
import { Flame, ShieldAlert, Activity, ArrowUpRight } from 'lucide-react';

// Component to handle map re-centering
function MapController({ center, zoom }) {
  const map = useMap();
  useEffect(() => {
    map.setView(center, zoom);
  }, [center, zoom, map]);
  return null;
}

// Function to create glowing HTML markers based on risk level
const createCustomMarkerIcon = (riskLevel, riskScore, isSelected) => {
  let color = '#10b981';
  let glowColor = 'rgba(16, 185, 129, 0.4)';

  if (riskLevel === 'Critical') {
    color = '#ef4444';
    glowColor = 'rgba(239, 68, 68, 0.6)';
  } else if (riskLevel === 'High') {
    color = '#f97316';
    glowColor = 'rgba(249, 115, 22, 0.5)';
  } else if (riskLevel === 'Moderate') {
    color = '#eab308';
    glowColor = 'rgba(234, 179, 8, 0.4)';
  }

  const size = isSelected ? 34 : 26;
  const borderStyle = isSelected ? '3px solid #ffffff' : '2px solid rgba(255,255,255,0.85)';

  const html = `
    <div style="
      position: relative;
      width: ${size}px;
      height: ${size}px;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
    ">
      <div style="
        position: absolute;
        width: 100%;
        height: 100%;
        border-radius: 50%;
        background-color: ${color};
        box-shadow: 0 0 16px ${glowColor};
        border: ${borderStyle};
        display: flex;
        align-items: center;
        justify-content: center;
        color: #ffffff;
        font-weight: 800;
        font-size: ${isSelected ? '12px' : '10px'};
        font-family: 'Outfit', sans-serif;
      ">
        ${Math.round(riskScore * 100)}
      </div>
      ${riskLevel === 'Critical' ? `
        <div style="
          position: absolute;
          width: ${size + 14}px;
          height: ${size + 14}px;
          border-radius: 50%;
          border: 2px solid ${color};
          animation: pulse-ring 2s infinite ease-out;
        "></div>
      ` : ''}
    </div>
  `;

  return L.divIcon({
    html: html,
    className: 'custom-map-marker',
    iconSize: [size, size],
    iconAnchor: [size / 2, size / 2],
    popupAnchor: [0, -size / 2],
  });
};

export default function MapView({ neighborhoods, selectedTract, onSelectTract, mode }) {
  // Phoenix Central coordinates
  const defaultCenter = [33.4750, -112.0800];
  const defaultZoom = 11;

  const getCircleColor = (tract) => {
    if (mode === 'thermal') {
      return tract.tempAvgF >= 110 ? '#ef4444' : tract.tempAvgF >= 105 ? '#f97316' : '#10b981';
    }
    if (mode === 'health') {
      // Dynamic scaling: aware of High Blood Pressure scale (30-40%) vs Asthma/CHD scale (5-15%)
      const isHighScale = tract.healthIndicator?.toUpperCase().includes('BP') || tract.healthValue > 22;
      const highCutoff = isHighScale ? 34 : 12;
      const medCutoff = isHighScale ? 28 : 9;

      return tract.healthValue >= highCutoff ? '#c084fc' : tract.healthValue >= medCutoff ? '#818cf8' : '#38bdf8';
    }
    // Combined Mode
    return tract.riskLevel === 'Critical' ? '#ef4444' : tract.riskLevel === 'High' ? '#f97316' : tract.riskLevel === 'Moderate' ? '#eab308' : '#10b981';
  };

  return (
    <div className="glass-panel" style={{ height: '580px', width: '100%', overflow: 'hidden', position: 'relative' }}>
      <MapContainer
        center={defaultCenter}
        zoom={defaultZoom}
        scrollWheelZoom={true}
        style={{ height: '100%', width: '100%' }}
      >
        <MapController center={defaultCenter} zoom={defaultZoom} />

        {/* High contrast CartoDB Dark Matter tile layer */}
        <TileLayer
          attribution='&copy; <a href="https://carto.com/">CARTO</a> &copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
          url="https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png"
          subdomains="abcd"
          maxZoom={19}
        />

        {neighborhoods.map((tract) => {
          const isSelected = selectedTract?.locationId === tract.locationId;
          const circleColor = getCircleColor(tract);
          const radiusSize = tract.riskLevel === 'Critical' ? 2200 : tract.riskLevel === 'High' ? 1800 : 1400;
          const correlationVal = tract.citywideCorrelation ?? tract.correlation ?? 0.84;

          return (
            <React.Fragment key={tract.locationId}>
              {/* Thermal Vulnerability Heat Radius */}
              <Circle
                center={[tract.latitude, tract.longitude]}
                radius={radiusSize}
                pathOptions={{
                  color: circleColor,
                  fillColor: circleColor,
                  fillOpacity: isSelected ? 0.25 : 0.12,
                  weight: isSelected ? 2 : 1,
                  dashArray: isSelected ? '4, 4' : undefined,
                }}
              />

              {/* Pulsing Tract Marker */}
              <Marker
                position={[tract.latitude, tract.longitude]}
                icon={createCustomMarkerIcon(tract.riskLevel, tract.riskScore, isSelected)}
                eventHandlers={{
                  click: () => onSelectTract(tract),
                }}
              >
                {/* Tooltip on Hover */}
                <Tooltip direction="top" offset={[0, -10]} opacity={0.95}>
                  <div style={{ fontWeight: 700, fontSize: '0.85rem' }}>{tract.name}</div>
                  <div style={{ fontSize: '0.75rem', color: '#6b7280' }}>
                    Temp: <b style={{ color: '#ef4444' }}>{tract.tempAvgF}°F</b> | {tract.healthIndicator || 'Health'}: <b>{tract.healthValue}%</b>
                  </div>
                </Tooltip>

                {/* Rich Popup */}
                <Popup>
                  <div style={{ minWidth: '200px' }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '6px' }}>
                      <span style={{ fontSize: '0.7rem', fontWeight: 800, padding: '2px 6px', borderRadius: '4px', background: `${circleColor}25`, color: circleColor, border: `1px solid ${circleColor}50` }}>
                        {tract.riskLevel} Risk
                      </span>
                      <span style={{ fontSize: '0.75rem', fontWeight: 700, color: '#38bdf8' }}>
                        Index: {(tract.riskScore * 100).toFixed(0)}%
                      </span>
                    </div>

                    <h4 style={{ fontSize: '0.95rem', fontWeight: 800, color: '#ffffff', marginBottom: '6px' }}>
                      {tract.name}
                    </h4>

                    <div style={{ fontSize: '0.8rem', display: 'flex', flexDirection: 'column', gap: '4px', margin: '8px 0', borderTop: '1px solid rgba(255,255,255,0.1)', paddingTop: '6px' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                        <span style={{ color: '#9ca3af' }}>Avg Peak Temp:</span>
                        <span style={{ fontWeight: 700, color: '#f87171' }}>{tract.tempAvgF}°F</span>
                      </div>
                      <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                        <span style={{ color: '#9ca3af' }}>{tract.healthIndicator || 'CDC Health'}:</span>
                        <span style={{ fontWeight: 700, color: '#c084fc' }}>{tract.healthValue}%</span>
                      </div>
                      <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                        <span style={{ color: '#9ca3af' }}>Citywide Spatial Corr:</span>
                        <span style={{ fontWeight: 700, color: '#34d399' }}>r = {correlationVal}</span>
                      </div>
                    </div>

                    <button
                      onClick={() => onSelectTract(tract)}
                      style={{
                        width: '100%',
                        padding: '6px 10px',
                        marginTop: '6px',
                        borderRadius: '6px',
                        background: 'linear-gradient(135deg, #ef4444, #f97316)',
                        border: 'none',
                        color: '#ffffff',
                        fontSize: '0.75rem',
                        fontWeight: 700,
                        cursor: 'pointer',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        gap: '4px'
                      }}
                    >
                      View Intelligence <ArrowUpRight size={13} />
                    </button>
                  </div>
                </Popup>
              </Marker>
            </React.Fragment>
          );
        })}
      </MapContainer>
    </div>
  );
}
