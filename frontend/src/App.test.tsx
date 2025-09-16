import React from 'react';
import { render } from '@testing-library/react';

test('basic test to ensure setup works', () => {
  const component = render(<div>Test</div>);
  expect(component).toBeTruthy();
});
