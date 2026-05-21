import L from "leaflet";
import "./mapControls.css";

export type MapResetControlOptions = {
  onReset: () => void;
  title: string;
};

/** Appends ↺ into the default zoom bar (+ / −) as a third control. */
export function attachMapResetControl(map: L.Map, options: MapResetControlOptions): () => void {
  const zoom = map.zoomControl;
  if (!zoom) return () => {};

  const container = zoom.getContainer();
  const link = L.DomUtil.create("a", "leaflet-control-zoom-reset", container);
  link.href = "#";
  link.setAttribute("role", "button");
  link.innerHTML = "↺";
  link.title = options.title;
  link.setAttribute("aria-label", options.title);

  const onClick = (e: Event) => {
    L.DomEvent.preventDefault(e);
    L.DomEvent.stopPropagation(e);
    options.onReset();
  };

  L.DomEvent.disableClickPropagation(link);
  L.DomEvent.on(link, "click", onClick);

  return () => {
    L.DomEvent.off(link, "click", onClick);
    if (link.parentNode === container) {
      container.removeChild(link);
    }
  };
}
