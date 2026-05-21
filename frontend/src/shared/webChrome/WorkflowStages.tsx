import { chrome } from "./styles";

export type WorkflowStage = {
  n: number;
  label: string;
  actor: string;
  action: string;
  status: string;
  done: boolean;
  path?: string;
};

type WorkflowStagesProps = {
  stages: WorkflowStage[];
  onNavigate: (path: string) => void;
};

export function WorkflowStages({ stages, onNavigate }: WorkflowStagesProps) {
  return (
    <div style={chrome.stageList}>
      {stages.map((s, i) => (
        <div key={s.n} style={chrome.stageItem}>
          <div
            style={{
              ...chrome.stageNum,
              background: s.done ? "#15803d" : "#cbd5e1",
              color: s.done ? "#fff" : "#475569",
            }}
          >
            {s.n}
          </div>
          {i < stages.length - 1 ? (
            <div
              style={{
                ...chrome.stageLine,
                background: s.done ? "#15803d" : "#e2e8f0",
              }}
            />
          ) : null}
          <div style={chrome.stageBody}>
            <div style={{ display: "flex", alignItems: "baseline", gap: 12 }}>
              <h3 style={chrome.stageTitle}>{s.label}</h3>
              <span style={chrome.stageActor}>{s.actor}</span>
            </div>
            <div style={{ display: "flex", alignItems: "center", gap: 16, marginTop: 6 }}>
              <span
                style={{
                  fontSize: 12,
                  color: "#64748b",
                  fontFamily: "ui-sans-serif, system-ui, -apple-system, sans-serif",
                }}
              >
                {s.status}
              </span>
              {s.path ? (
                <button type="button" onClick={() => onNavigate(s.path!)} style={chrome.linkBtn}>
                  {s.action} →
                </button>
              ) : null}
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
