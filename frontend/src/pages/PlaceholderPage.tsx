import { Typography } from "antd";

export function PlaceholderPage({ titolo }: { titolo: string }) {
  return (
    <div>
      <Typography.Title level={2}>{titolo}</Typography.Title>
      <Typography.Paragraph>Sezione in costruzione.</Typography.Paragraph>
    </div>
  );
}
