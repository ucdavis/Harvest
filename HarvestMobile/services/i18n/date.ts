import moment from 'moment';
import * as config from '../../config/i18n';

const date = {
    init(locale: config.supportedLocaleNames) {
        return new Promise((resolve, reject) => {
            config
                .supportedLocales[locale]
                .momentLocaleLoader()
                .then(() => {
                    moment.locale(locale);
                    return resolve();
                })
                .catch(err => reject(err));
        });
    },

    format(date: Date, format: string) {
        return moment(date).format(format);
    }
}

export default date;