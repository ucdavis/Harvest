import * as React from 'react';
import { jest } from '@jest/globals';

import { Text, TextProps } from './Themed';

jest.useFakeTimers();

export function MonoText(props: TextProps) {
  return <Text {...props} style={[props.style, { fontFamily: 'space-mono' }]} />;
}
