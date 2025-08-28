import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'EntrevistaABP',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://localhost:44356/',
    redirectUri: baseUrl,
    clientId: 'EntrevistaABP_App',
    responseType: 'code',
    scope: 'offline_access EntrevistaABP',
    requireHttps: true
  },
  apis: {
    default: {
      url: 'https://localhost:44356',
      rootNamespace: 'WB.EntrevistaABP',
    },
  },
} as Environment;
