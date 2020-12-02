import i18next from 'i18next';
import * as config from '../../config/i18n';
import date from './date';
import languageDetector from './language-detector';
import translationLoader from './translation-loader';
import { initReactI18next } from 'react-i18next';

const i18n = {
  /**
   * @returns {Promise}
   */
  init: () => {
    return new Promise((resolve, reject) => {
      i18next
        .use(languageDetector)
        .use(translationLoader)
        .use(initReactI18next)
        .init({
          fallbackLng: config.fallback,
          ns: config.namespaces,
          defaultNS: config.defaultNamespace,
          interpolation: {
            escapeValue: false,
            format: (value, format) => {
              if (value instanceof Date) {
                return date.format(value, format || 'MM-DD-YYYY');
              }
              return value;
            }
          },
        }, (error) => {
          if (error) { return reject(error); }
          date.init(i18next.language as config.supportedLocaleNames)
            .then(resolve)
            .catch(error => reject(error));
        });
    });
  }
}

export default i18n;