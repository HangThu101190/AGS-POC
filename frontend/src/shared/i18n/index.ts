import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import en from "./locales/en.json";
import vi from "./locales/vi.json";

const saved = localStorage.getItem("ags.lang");
const lng = saved === "en" ? "en" : "vi";

void i18n.use(initReactI18next).init({
  resources: {
    vi: { translation: vi },
    en: { translation: en },
  },
  lng,
  fallbackLng: "vi",
  interpolation: { escapeValue: false },
});

export default i18n;
