import i18n from "@/shared/i18n";

export function applyPreferredLanguage(lang: "vi" | "en") {
  void i18n.changeLanguage(lang);
  localStorage.setItem("ags.lang", lang);
}
