import { StatusBar } from 'expo-status-bar';
import React, { useEffect } from 'react';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import * as WebBrowser from "expo-web-browser";

import useCachedResources from './hooks/useCachedResources';
import useColorScheme from './hooks/useColorScheme';
import Navigation from './navigation';
import i18n from './services/i18n';
import { useState } from '@hookstate/core';
import { useLocale } from './hooks/useLocale';

WebBrowser.maybeCompleteAuthSession();

export default function App() {
  const isLoadingComplete = useCachedResources();
  const colorScheme = useColorScheme();
  const isI18nInitialized = useState(false);

  useEffect(() => {
    i18n.init()
      .then(() => {
        isI18nInitialized.set(true);
      })
      .catch(error => console.warn(error));
  }, [])

  if (!isLoadingComplete || !isI18nInitialized.get()) {
    return null;
  } else {
    return (
      <SafeAreaProvider>
        <Navigation colorScheme={colorScheme} />
        <StatusBar />
      </SafeAreaProvider>
    );
  }
}
