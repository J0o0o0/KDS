export const environment = {
  production: false,
  // .NET backend — HTTP port (avoids the self-signed HTTPS cert warning).
  // launchSettings.json exposes: http://localhost:5050 and https://localhost:7050
  apiUrl: 'http://localhost:5050/api',
  hubUrl: 'http://localhost:5050/hubs/kitchen',
};
