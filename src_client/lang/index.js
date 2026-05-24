import ruLanguage from './ru'
import enLanguage from './en'

const language = {
    'ru': ruLanguage,
    'en': enLanguage
}

let currentLang = 'en';

global.translateText = function (text) {
    const languageText = language[currentLang][text];

    if (languageText)
        text = languageText;

    const args = Array.prototype.slice.call(arguments, 1);
    return text.replace(/{(\d+)}/g, function (match, number) {
        return typeof args[number] != 'undefined' ? args[number] : match;
    });
};