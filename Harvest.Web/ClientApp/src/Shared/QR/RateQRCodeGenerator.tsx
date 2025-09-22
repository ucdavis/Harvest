import React from "react";
import { Rate } from "../../types";
import { useQRCodeGeneration } from "./useQRCodeGeneration";
import { QRCodeModal } from "./QRCodeModal";
import { printQRCode, PrintDetail } from "./printUtils";

interface RateQRCodeGeneratorProps {
  rate: Rate;
  team?: string; // For backwards compatibility, this will be the slug
  teamInfo?: {
    name: string;
    slug: string;
  };
  onClose: () => void;
}

export const RateQRCodeGenerator: React.FC<RateQRCodeGeneratorProps> = ({
  rate,
  team,
  teamInfo,
  onClose,
}) => {
  // Build the URL that the QR code will contain
  const baseUrl = window.location.origin;
  const teamSlug = teamInfo?.slug || team || "unknown";
  const rateUrl = `${baseUrl}/${teamSlug}/Rate/Details/${rate.id}`;

  const { canvasRef, qrCodeGenerated, qrDataUrl, error } =
    useQRCodeGeneration(rateUrl);

  const handlePrint = () => {
    if (!qrDataUrl) {
      console.error("QR Code not generated yet");
      return;
    }

    const printDetails: PrintDetail[] = [
      { label: "Type", value: rate.type },
      { label: "Description", value: rate.description },
      { label: "Unit", value: rate.unit || "N/A" },
      { label: "Price", value: `$${rate.price.toFixed(2)}` },
      { label: "Passthrough", value: rate.isPassthrough ? "Yes" : "No" },
    ];

    if (teamInfo) {
      printDetails.push({
        label: "Team",
        value: `${teamInfo.name} (${teamInfo.slug})`,
      });
    } else if (team) {
      printDetails.push({ label: "Team", value: team });
    }

    printQRCode({
      title: "UC Davis Harvest",
      subtitle: "Rate QR Code",
      itemId: rate.id,
      itemName: rate.description,
      details: printDetails,
      qrDataUrl,
      instructions:
        "Scan this QR code with your mobile device to quickly access this rate information.",
    });
  };

  return (
    <QRCodeModal
      title="Rate QR Code"
      itemName={rate.description}
      itemId={rate.id}
      onClose={onClose}
      onPrint={handlePrint}
      qrCodeGenerated={qrCodeGenerated}
      qrDataUrl={qrDataUrl}
    >
      {error ? (
        <div className="alert alert-danger">
          <strong>Error generating QR code:</strong> {error}
        </div>
      ) : qrCodeGenerated ? (
        <div>
          <canvas
            ref={canvasRef}
            style={{ border: "1px solid #ddd", borderRadius: "4px" }}
          />
          <div className="mt-3">
            <p className="small text-muted">
              Scan this QR code to quickly access this rate information on
              mobile devices.
            </p>
            <div className="small text-info">
              <strong>Rate Details:</strong>
              <br />
              Type: {rate.type} | Price: ${rate.price.toFixed(2)}
              {rate.unit && ` per ${rate.unit}`}
            </div>
          </div>
        </div>
      ) : (
        <div>
          <canvas ref={canvasRef} style={{ display: "none" }} />
          <p>Generating QR code...</p>
        </div>
      )}
    </QRCodeModal>
  );
};
