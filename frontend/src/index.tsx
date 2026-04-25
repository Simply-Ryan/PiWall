import { registerRootComponent } from 'expo';
import React from 'react';
import MinimalApp from './MinimalApp';
import { ErrorBoundary } from './ErrorBoundary';

// Wrap app with error boundary to catch any rendering errors
const RootApp = () => (
  <ErrorBoundary>
    <MinimalApp />
  </ErrorBoundary>
);

registerRootComponent(RootApp);
