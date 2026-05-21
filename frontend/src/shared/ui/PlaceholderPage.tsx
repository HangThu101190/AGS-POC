import Typography from "@mui/material/Typography";
import { useTranslation } from "react-i18next";
import { Card } from "@/components/ui";

interface PlaceholderPageProps {
  title: string;
}

export function PlaceholderPage({ title: _title }: PlaceholderPageProps) {
  const { t } = useTranslation();
  return (
    <Card>
      <Typography color="text.secondary">{t("common.comingSoon")}</Typography>
    </Card>
  );
}
