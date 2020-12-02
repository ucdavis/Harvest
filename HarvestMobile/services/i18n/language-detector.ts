import * as Localization from 'expo-localization';
import { Module } from 'i18next';

const languageDetector = {
    type: 'languageDetector',
    async: true,
    detect: (callback: (locale: string) => void) => {
        callback(Localization.locale.split('-')[0]);
    },
    init: () => { },
    cacheUserLanguage: () => { },
} as Module;

export default languageDetector;