import en from './texts/en.json';

export type SupportedLocale = 'en';

export const defaultLocale: SupportedLocale = 'en';

// Messages by locale
const messagesByLocale: Record<SupportedLocale, Record<string, string>> = {
  en,
};

export const getMessages = (locale: SupportedLocale): Record<string, string> => {
  return messagesByLocale[locale] || en;
};

// Legacy export for backwards compatibility
export const messages = en;

export const locales: Record<SupportedLocale, string> = {
  en: 'English',
};
