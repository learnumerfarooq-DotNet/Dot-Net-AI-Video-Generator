/**
 * API base URL and endpoint constants.
 *
 * Rules:
 *  - API_BASE   → the single place to change the server origin
 *  - ENDPOINTS  → the base entity paths
 */

export const API_BASE = 'http://localhost:5039';

export const ENDPOINTS = {
  WORKSPACE: 'api/workspace',
  AGENTS: 'api/agents',
  MEMORY: 'api/memory',
  VIDEOS: 'api/videos',
  SCHEDULER: 'api/scheduler',
  SETTINGS: 'api/settings',
  DRIVE: 'api/drive',
  DASHBOARD_SUMMARY: 'api/dashboard/summary',
  DASHBOARD_VIDEOS: 'api/dashboard/videos',
  DASHBOARD_RUNS: 'api/dashboard/runs'
} as const;
