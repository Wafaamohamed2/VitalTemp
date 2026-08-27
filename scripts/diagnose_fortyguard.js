const https = require('https');

const API_KEY = process.env.FORTYGUARD_API_KEY || process.argv[2] || "";

if (!API_KEY) {
  console.error("FortyGuard API Key is required.");
  console.log("Usage: node diagnose_fortyguard.js <YOUR_API_KEY> or set FORTYGUARD_API_KEY env var");
  process.exit(1);
}

function testEndpoint(name, urlStr, method = 'GET', payload = null) {
  return new Promise((resolve) => {
    const url = new URL(urlStr);
    const postData = payload ? JSON.stringify(payload) : null;
    const reqOptions = {
      hostname: url.hostname,
      port: 443,
      path: url.pathname + url.search,
      method: method,
      headers: {
        'api-key': API_KEY,
        ...(postData ? {
          'Content-Type': 'application/json',
          'Content-Length': Buffer.byteLength(postData)
        } : {})
      }
    };

    const req = https.request(reqOptions, (res) => {
      let body = '';
      res.on('data', chunk => body += chunk);
      res.on('end', () => {
        console.log(`[${name}] Status: ${res.statusCode} | Body: ${body.substring(0, 200)}`);
        resolve({ name, status: res.statusCode, body });
      });
    });

    req.on('error', (err) => {
      console.log(`[${name}] Error: ${err.message}`);
      resolve({ name, error: err.message });
    });

    if (postData) req.write(postData);
    req.end();
  });
}

async function runDiagnosis() {
  console.log("--- FortyGuard Diagnostic Suite ---");
  await testEndpoint("Root /", "https://api.fortyguard.com/", "GET");
  await testEndpoint("Health check", "https://api.fortyguard.com/health", "GET");
  await testEndpoint("v1 Root", "https://api.fortyguard.com/v1", "GET");
}

runDiagnosis();
