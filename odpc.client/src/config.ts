// Configuration for the ODPC client
export const config = {
  // Production (empty string): Uses relative URLs like /api/v1/metadata/health
  //   - Requests go through nginx which routes /api/v1/metadata/ to ODPC
  // Local dev: Set VITE_ODPC_API_URL=http://localhost:62230 in .env.local
  //   - Bypasses nginx, calls ODPC directly to avoid CORS
  odpcApiUrl: import.meta.env.VITE_ODPC_API_URL || ""
};
