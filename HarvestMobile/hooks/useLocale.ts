import useAppState from 'react-native-appstate-hook';
import { Platform } from 'react-native';
import { useState } from '@hookstate/core';
import * as Localization from 'expo-localization';

export const useLocale = () => {
  const locale = useState<string>(Localization.locale);

  // Expo docs say ios resets all apps on locale change but that android needs a nudge.
  // Not sure this is true in the case of react-i18next, as it appears to be resetting
  // on android after changing language.
  useAppState({
    onForeground: () => {
      if (Platform.OS == "android")
        Localization.getLocalizationAsync()
          .then((localization: Localization.Localization) => locale.set(localization.locale))
    },
  });

  return locale.get();
};