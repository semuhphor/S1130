import React from 'react';
import { render, screen } from '@testing-library/react';
import App from './App';

test('renders the emulator page', () => {
  render(<App />);
  expect(screen.getByRole('heading', { name: /IBM 1130 Emulator/i })).toBeInTheDocument();
});
