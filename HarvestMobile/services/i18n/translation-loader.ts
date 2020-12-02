import { Module } from 'i18next';
import * as config from '../../config/i18n';

const translationLoader = {
    type: 'backend',
    init: () => {},
    read: function(language: config.supportedLocaleNames, namespace: string, callback: (error: any, resource: any) => void) {
        let resource, error = null;
        try {
            resource = config
                .supportedLocales[language]
                .translationFileLoader()[namespace];
        } catch (_error) { error = _error; }
        callback(error, resource);
    },
} as Module;

export default translationLoader;