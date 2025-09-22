import { useEffect, useRef, useState, useMemo } from "react";
import QRCode from "qrcode";

// Helper function to draw rounded rectangles
const drawRoundedRect = (
  ctx: CanvasRenderingContext2D,
  x: number,
  y: number,
  width: number,
  height: number,
  radius: number
) => {
  ctx.beginPath();
  ctx.moveTo(x + radius, y);
  ctx.lineTo(x + width - radius, y);
  ctx.quadraticCurveTo(x + width, y, x + width, y + radius);
  ctx.lineTo(x + width, y + height - radius);
  ctx.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
  ctx.lineTo(x + radius, y + height);
  ctx.quadraticCurveTo(x, y + height, x, y + height - radius);
  ctx.lineTo(x, y + radius);
  ctx.quadraticCurveTo(x, y, x + radius, y);
  ctx.closePath();
};

export interface QRCodeGenerationOptions {
  errorCorrectionLevel?: "L" | "M" | "Q" | "H";
  margin?: number;
  width?: number;
  color?: {
    dark?: string;
    light?: string;
  };
  logo?: {
    src: string; // URL or data URL of the logo image
    size?: number; // Size of the logo as percentage of QR code (default: 20%)
    borderRadius?: number; // Border radius in pixels
    backgroundColor?: string; // Background color behind logo
    padding?: number; // Padding around logo in pixels
  };
}

export interface UseQRCodeGenerationResult {
  canvasRef: React.RefObject<HTMLCanvasElement>;
  qrCodeGenerated: boolean;
  qrDataUrl: string;
  error: string | null;
}

export const useQRCodeGeneration = (
  url: string,
  options: QRCodeGenerationOptions = {}
): UseQRCodeGenerationResult => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [qrCodeGenerated, setQrCodeGenerated] = useState(false);
  const [qrDataUrl, setQrDataUrl] = useState<string>("");
  const [error, setError] = useState<string | null>(null);

  const defaultOptions = useMemo(
    (): QRCodeGenerationOptions => ({
      errorCorrectionLevel: "H", // Use high error correction for logos
      margin: 1,
      color: {
        dark: "#000000",
        light: "#FFFFFF",
      },
      width: 256,
      ...options,
    }),
    [options]
  );

  useEffect(() => {
    const addLogoToQRCode = (
      canvas: HTMLCanvasElement,
      logoOptions: NonNullable<QRCodeGenerationOptions["logo"]>
    ) => {
      return new Promise<void>((resolve, reject) => {
        const ctx = canvas.getContext("2d");
        if (!ctx) {
          reject(new Error("Could not get canvas context"));
          return;
        }

        const img = new Image();
        img.crossOrigin = "anonymous";

        img.onload = () => {
          const canvasSize = canvas.width;
          const logoSize = logoOptions.size || 20; // Default 20% of QR code size
          const logoPixelSize = (canvasSize * logoSize) / 100;
          const logoX = (canvasSize - logoPixelSize) / 2;
          const logoY = (canvasSize - logoPixelSize) / 2;
          const padding = logoOptions.padding || 4;
          const borderRadius = logoOptions.borderRadius || 8;

          // Draw background circle/rounded rectangle if specified
          if (logoOptions.backgroundColor) {
            ctx.fillStyle = logoOptions.backgroundColor;
            const bgSize = logoPixelSize + padding * 2;
            const bgX = logoX - padding;
            const bgY = logoY - padding;

            if (borderRadius > 0) {
              drawRoundedRect(ctx, bgX, bgY, bgSize, bgSize, borderRadius);
              ctx.fill();
            } else {
              ctx.fillRect(bgX, bgY, bgSize, bgSize);
            }
          }

          // Draw the logo image
          ctx.save();
          if (borderRadius > 0) {
            drawRoundedRect(
              ctx,
              logoX,
              logoY,
              logoPixelSize,
              logoPixelSize,
              borderRadius
            );
            ctx.clip();
          }

          ctx.drawImage(img, logoX, logoY, logoPixelSize, logoPixelSize);
          ctx.restore();

          resolve();
        };

        img.onerror = () => {
          reject(new Error(`Failed to load logo image: ${logoOptions.src}`));
        };

        img.src = logoOptions.src;
      });
    };

    const generateQRCode = async () => {
      if (canvasRef.current && url) {
        try {
          setError(null);

          // Generate the base QR code
          await QRCode.toCanvas(canvasRef.current, url, defaultOptions);

          // Add logo if specified
          if (defaultOptions.logo) {
            await addLogoToQRCode(canvasRef.current, defaultOptions.logo);
          }

          // Store the data URL for printing
          const dataUrl = canvasRef.current.toDataURL("image/png", 1.0);
          setQrDataUrl(dataUrl);
          setQrCodeGenerated(true);
        } catch (err) {
          console.error("Error generating QR code:", err);
          setError(err instanceof Error ? err.message : "Unknown error");
          setQrCodeGenerated(false);
        }
      }
    };

    generateQRCode();
  }, [url, defaultOptions]);

  return {
    canvasRef,
    qrCodeGenerated,
    qrDataUrl,
    error,
  };
};
