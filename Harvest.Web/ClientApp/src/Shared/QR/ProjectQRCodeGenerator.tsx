import React from "react";
import { Project } from "../../types";
import { useQRCodeGeneration } from "./useQRCodeGeneration";
import { QRCodeModal } from "./QRCodeModal";
import { printQRCode, PrintDetail } from "./printUtils";
import { createProjectLeafLogo } from "./qrLogos";

interface ProjectQRCodeGeneratorProps {
  project: Project;
  team?: string;
  onClose: () => void;
}

export const ProjectQRCodeGenerator: React.FC<ProjectQRCodeGeneratorProps> = ({
  project,
  team,
  onClose,
}) => {
  // Build the URL that the QR code will contain
  const baseUrl = window.location.origin;
  const teamSlug = team || project.team?.slug || "unknown";
  const projectUrl = `${baseUrl}/${teamSlug}/project/details/${project.id}`;

  const { canvasRef, qrCodeGenerated, qrDataUrl, error } = useQRCodeGeneration(
    projectUrl,
    {
      logo: {
        src: createProjectLeafLogo(48),
        size: 18, // 18% of QR code size
        backgroundColor: "#ffffff",
        padding: 4,
        borderRadius: 8,
      },
    }
  );

  const handlePrint = () => {
    if (!qrDataUrl) {
      console.error("QR Code not generated yet");
      return;
    }

    const printDetails: PrintDetail[] = [
      {
        label: "Principal Investigator",
        value: project.principalInvestigator?.name || "N/A",
      },
      {
        label: "Timeline",
        value: `${new Date(
          project.start
        ).toLocaleDateString()} through ${new Date(
          project.end
        ).toLocaleDateString()}`,
      },
      {
        label: "Team",
        value: `${project.team?.name || team} (${project.team?.slug || team})`,
      },
    ];

    printQRCode({
      title: "UC Davis Harvest",
      subtitle: "Field Request QR Code",
      itemId: project.id,
      itemName: project.name,
      details: printDetails,
      qrDataUrl,
      instructions:
        "Scan this QR code with your mobile device to quickly access this project.",
    });
  };

  return (
    <QRCodeModal
      title="Project QR Code"
      itemName={project.name}
      itemId={project.id}
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
              Scan this QR code to quickly access this project on mobile
              devices.
            </p>
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
