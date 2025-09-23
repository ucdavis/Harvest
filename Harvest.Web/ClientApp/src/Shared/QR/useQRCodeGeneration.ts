import { useEffect, useRef, useState, useMemo } from "react";
import QRCode from "qrcode";

export interface QRCodeGenerationOptions {
  errorCorrectionLevel?: "L" | "M" | "Q" | "H";
  margin?: number;
  width?: number;
  color?: {
    dark?: string;
    light?: string;
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
      errorCorrectionLevel: "M",
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
    const generateQRCode = async () => {
      if (canvasRef.current && url) {
        try {
          setError(null);
          await QRCode.toCanvas(canvasRef.current, url, defaultOptions);

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
