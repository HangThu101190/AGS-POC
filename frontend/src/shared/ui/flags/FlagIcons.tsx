type FlagProps = {
  className?: string;
  title?: string;
};

/** 5-point star centered in viewBox (cx, cy). */
function starPoints(cx: number, cy: number, outerR: number, innerR: number): string {
  const spikes = 5;
  const step = Math.PI / spikes;
  let rot = (Math.PI / 2) * 3;
  const pts: string[] = [];
  for (let i = 0; i < spikes; i++) {
    pts.push(`${cx + Math.cos(rot) * outerR},${cy + Math.sin(rot) * outerR}`);
    rot += step;
    pts.push(`${cx + Math.cos(rot) * innerR},${cy + Math.sin(rot) * innerR}`);
    rot += step;
  }
  return pts.join(" ");
}

export function VietnamFlag({ className, title }: FlagProps) {
  return (
    <svg
      className={className}
      viewBox="0 0 28 20"
      xmlns="http://www.w3.org/2000/svg"
      role="img"
      aria-hidden={title ? undefined : true}
      aria-label={title}
      preserveAspectRatio="xMidYMid meet"
    >
      <rect width="28" height="20" fill="#da251d" rx="2" />
      <polygon fill="#ffde00" points={starPoints(14, 10, 5.5, 2.3)} />
    </svg>
  );
}

export function GbFlag({ className, title }: FlagProps) {
  return (
    <svg
      className={className}
      viewBox="0 0 28 20"
      xmlns="http://www.w3.org/2000/svg"
      role="img"
      aria-hidden={title ? undefined : true}
      aria-label={title}
      preserveAspectRatio="xMidYMid meet"
    >
      <rect width="28" height="20" fill="#012169" rx="2" />
      <path d="M0 0 L28 20 M28 0 L0 20" stroke="#fff" strokeWidth="3.2" />
      <path d="M0 0 L28 20 M28 0 L0 20" stroke="#c8102e" strokeWidth="1.6" />
      <path d="M14 0 V20 M0 10 H28" stroke="#fff" strokeWidth="5.2" />
      <path d="M14 0 V20 M0 10 H28" stroke="#c8102e" strokeWidth="3.2" />
    </svg>
  );
}
