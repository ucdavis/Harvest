// QR Code logo utilities for Harvest application

/**
 * Creates a leaf icon SVG for project QR codes
 */
export const createProjectLeafLogo = (size: number = 48): string => {
  const svg = `
    <svg width="${size}" height="${size}" viewBox="0 0 48 48" xmlns="http://www.w3.org/2000/svg">
      <defs>
        <radialGradient id="leafGradient" cx="0.3" cy="0.3" r="0.8">
          <stop offset="0%" style="stop-color:#4ade80;stop-opacity:1" />
          <stop offset="100%" style="stop-color:#15803d;stop-opacity:1" />
        </radialGradient>
      </defs>
      <!-- Leaf shape -->
      <path d="M24 6 C32 6, 42 14, 42 24 C42 32, 36 38, 30 40 L24 42 C20 38, 16 32, 12 24 C12 16, 16 8, 24 6 Z" 
            fill="url(#leafGradient)" 
            stroke="#166534" 
            stroke-width="1"/>
      <!-- Leaf vein -->
      <path d="M24 8 Q28 16, 30 24 Q28 32, 24 40" 
            stroke="#166534" 
            stroke-width="1.5" 
            fill="none"/>
      <!-- Secondary veins -->
      <path d="M20 16 Q24 18, 28 20" stroke="#166534" stroke-width="0.8" fill="none" opacity="0.7"/>
      <path d="M18 24 Q24 26, 30 28" stroke="#166534" stroke-width="0.8" fill="none" opacity="0.7"/>
      <path d="M20 32 Q24 34, 28 36" stroke="#166534" stroke-width="0.8" fill="none" opacity="0.7"/>
    </svg>
  `;

  return `data:image/svg+xml;base64,${btoa(svg)}`;
};

/**
 * Creates a tractor icon SVG for rate QR codes
 */
export const createRateTractorLogo = (size: number = 48): string => {
  const svg = `
    <svg width="${size}" height="${size}" viewBox="0 0 48 48" xmlns="http://www.w3.org/2000/svg">
      <defs>
        <linearGradient id="tractorGradient" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" style="stop-color:#dc2626;stop-opacity:1" />
          <stop offset="100%" style="stop-color:#991b1b;stop-opacity:1" />
        </linearGradient>
      </defs>
      <!-- Tractor body -->
      <rect x="14" y="18" width="20" height="12" rx="2" fill="url(#tractorGradient)" stroke="#7f1d1d" stroke-width="1"/>
      <!-- Cabin -->
      <rect x="26" y="12" width="8" height="12" rx="1" fill="url(#tractorGradient)" stroke="#7f1d1d" stroke-width="1"/>
      <!-- Large rear wheel -->
      <circle cx="28" cy="36" r="8" fill="#374151" stroke="#1f2937" stroke-width="1.5"/>
      <circle cx="28" cy="36" r="5" fill="none" stroke="#6b7280" stroke-width="1"/>
      <!-- Small front wheel -->
      <circle cx="16" cy="34" r="5" fill="#374151" stroke="#1f2937" stroke-width="1.5"/>
      <circle cx="16" cy="34" r="3" fill="none" stroke="#6b7280" stroke-width="1"/>
      <!-- Exhaust pipe -->
      <rect x="22" y="8" width="2" height="8" fill="#4b5563"/>
      <!-- Window -->
      <rect x="28" y="14" width="4" height="4" fill="#bfdbfe" opacity="0.8"/>
      <!-- Headlight -->
      <circle cx="12" cy="20" r="2" fill="#fbbf24"/>
    </svg>
  `;

  return `data:image/svg+xml;base64,${btoa(svg)}`;
};

/**
 * Creates a fallback UC Davis logo for general use
 */
export const createUCDavisLogo = (size: number = 48): string => {
  const svg = `
    <svg width="${size}" height="${size}" viewBox="0 0 48 48" xmlns="http://www.w3.org/2000/svg">
      <defs>
        <linearGradient id="ucdGradient" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" style="stop-color:#1e40af;stop-opacity:1" />
          <stop offset="100%" style="stop-color:#1e3a8a;stop-opacity:1" />
        </linearGradient>
      </defs>
      <!-- UC Davis circle -->
      <circle cx="24" cy="24" r="20" fill="url(#ucdGradient)" stroke="#1e3a8a" stroke-width="2"/>
      <!-- UC letters -->
      <text x="24" y="28" text-anchor="middle" font-family="Arial, sans-serif" font-size="12" font-weight="bold" fill="white">UC</text>
      <!-- Davis text -->
      <text x="24" y="38" text-anchor="middle" font-family="Arial, sans-serif" font-size="8" fill="white">DAVIS</text>
    </svg>
  `;

  return `data:image/svg+xml;base64,${btoa(svg)}`;
};
