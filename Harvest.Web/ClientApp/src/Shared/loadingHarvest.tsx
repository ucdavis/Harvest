import React from "react";

type Props = {
  /** Pixel size or CSS size (e.g., 64 or "2rem") */
  size?: number | string;
  /** Stroke color */
  color?: string;
  /** Stroke width in px */
  strokeWidth?: number;
  /** Rotation duration in seconds */
  durationSec?: number;
  /** Accessible label */
  label?: string;
};

const loadingHarvest: React.FC<Props> = ({
  size = 64,
  color = "#266041",
  strokeWidth = 4,
  durationSec = 1,
  label = "Loading",
}) => (
  <svg
    width={size}
    height={size}
    viewBox="0 0 50 50"
    xmlns="http://www.w3.org/2000/svg"
    role="img"
    aria-label={label}
  >
    <g fill="none" stroke={color} strokeWidth={strokeWidth}>
      <circle cx="25" cy="25" r="20" strokeOpacity="0.2" />
      <path d="M25 5 A20 20 0 0 1 45 25" strokeLinecap="round">
        <animateTransform
          attributeName="transform"
          type="rotate"
          from="0 25 25"
          to="360 25 25"
          dur={`${durationSec}s`}
          repeatCount="indefinite"
        />
      </path>
    </g>
  </svg>
);

export default loadingHarvest;