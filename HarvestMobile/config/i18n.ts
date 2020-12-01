export const fallback = "en";

export type supportedLocaleNames = "en" | "es";

export const supportedLocales = {
    "en": {
        name: "English",
        translationFileLoader: () => require('../assets/lang/en.json'),
        // en is default locale in Moment
        momentLocaleLoader: () => Promise.resolve(),
    },
    "es": {
        name: "Español",
        translationFileLoader: () => require('../assets/lang/es.json'),
        momentLocaleLoader: () => import('moment/locale/es'),
    },
};
export const defaultNamespace = "common";
export const namespaces = [
    "common",
];