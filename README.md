## VitalTemp — Phoenix Urban Heat & Health Vulnerability Intelligence

VitalTemp combines real FortyGuard thermal data with CDC PLACES health indicators across 100 real Phoenix County census tracts to identify neighborhoods facing the highest combined heat and health risk — and generates AI-powered, tract-specific mitigation recommendations via Google Gemini.

Live demo: https://vitaltemp.up.railway.app

## Architecture
    - Backend: ASP.NET Core, Clean Architecture (Domain / Application / Infrastructure / API), Entity Framework Core, SQLite
    - Frontend: React + Vite + Leaflet, served as a single unified app from the API's wwwroot
    - External APIs: FortyGuard Enterprise API (Heatmap Generation), Google Gemini 1.5 Flash

## How to Run It Locally
Prerequisites:
     .NET 10 SDK
     Node.js 18+
     A FortyGuard API key and a Google Gemini API key (optional — the app runs with calibrated fallback data if neither is configured)    

## Backend
     cd src/VitalTemp.API
     dotnet user-secrets set "FortyGuard:ApiKey" "YOUR_FORTYGUARD_KEY"
     dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_KEY"
     dotnet run
The API auto-applies EF Core migrations and seeds the database with the 100 real Phoenix tracts on first run. Swagger UI is available at /swagger in development.     
     
## Frontend (development mode only — not needed for the deployed build)     
    cd vitaltemp-dashboard
    npm install
    npm run dev


## One Real FortyGuard API Request + Response Example
   Request — POST https://api.fortyguard.com/v1/heatmap    
   ```json
   {
  "polygon_aoi": {
    "type": "FeatureCollection",
    "features": [{
      "type": "Feature",
      "properties": {},
      "geometry": {
        "type": "Polygon",
        "coordinates": [[
          [-112.1300, 33.4000],
          [-112.1300, 33.5000],
          [-112.0300, 33.5000],
          [-112.0300, 33.4000],
          [-112.1300, 33.4000]
        ]]
      }
    }]
  },
  "date_time": {
    "start_date": "2023-08-18",
    "start_time": "14:00",
    "filter_type": 1
  },
  "granularity": 100
}

Header: api-key: <FORTYGUARD_API_KEY>

Submit Response:
{
    "error": false,
    "status_code": 200,
    "message": "Heatmap Submitted Successfully",
    "data": {
      "activity_id": "1d66d7d6-27cf-4f0d-87d1-48df3e7171d4"
    }
}
```

## What Doesn't Work Yet
  - "Critical" risk tier is empty for the current 100-tract sample. This is a genuine finding, not a bug: the citywide heat-health correlation in this sample is slightly negative (r ≈ -0.27), so no single tract combines both extreme heat and extreme health burden. See the dashboard's correlation panel.
  - Real-time push updates (SignalR) are not implemented; the dashboard requires a manual refresh/sync to pick up new data.
