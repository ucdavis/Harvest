import React, { useEffect, useRef, useState } from "react";
import QRCode from "qrcode";
import { Project } from "../types";

interface QRCodeGeneratorProps {
  project: Project;
  team: string;
  onClose: () => void;
}

export const QRCodeGenerator: React.FC<QRCodeGeneratorProps> = ({
  project,
  team,
  onClose,
}) => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [qrCodeGenerated, setQrCodeGenerated] = useState(false);
  const [qrDataUrl, setQrDataUrl] = useState<string>("");

  useEffect(() => {
    const generateQRCode = async () => {
      if (canvasRef.current) {
        try {
          // Build the URL that the QR code will contain
          const baseUrl = window.location.origin;
          const projectUrl = `${baseUrl}/${team}/project/details/${project.id}`;

          // Generate QR code
          await QRCode.toCanvas(canvasRef.current, projectUrl, {
            errorCorrectionLevel: "M",
            margin: 1,
            color: {
              dark: "#000000",
              light: "#FFFFFF",
            },
            width: 256,
          });

          // Store the data URL for printing
          const dataUrl = canvasRef.current.toDataURL("image/png", 1.0);
          setQrDataUrl(dataUrl);
          setQrCodeGenerated(true);
        } catch (error) {
          console.error("Error generating QR code:", error);
        }
      }
    };

    generateQRCode();
  }, [project.id, team]);

  const handlePrint = () => {
    if (!qrDataUrl) {
      console.error("QR Code not generated yet");
      return;
    }

    const printWindow = window.open("", "_blank");
    if (printWindow) {
      const printContent = `
        <!DOCTYPE html>
        <html>
        <head>
          <title>Project QR Code - ${project.name}</title>
          <style>
            body {
              font-family: Arial, sans-serif;
              text-align: center;
              padding: 20px;
              margin: 0;
            }
            .qr-container {
              max-width: 600px;
              margin: 0 auto;
              padding: 30px;
              border: 2px solid #333;
              border-radius: 10px;
            }
            .qr-title {
              font-size: 24px;
              font-weight: bold;
              margin-bottom: 10px;
              color: #333;
            }
            .project-info {
              margin-bottom: 20px;
              font-size: 16px;
              color: #666;
            }
            .qr-code {
              margin: 20px 0;
            }
            .qr-code img {
              max-width: 100%;
              height: auto;
              border: 1px solid #ddd;
            }
            .instructions {
              font-size: 14px;
              color: #888;
              margin-top: 20px;
              line-height: 1.4;
            }
            .project-details {
              background-color: #f5f5f5;
              padding: 15px;
              border-radius: 5px;
              margin: 15px 0;
              text-align: left;
            }
            .detail-row {
              margin: 8px 0;
              display: flex;
              justify-content: space-between;
            }
            .detail-label {
              font-weight: bold;
              color: #333;
            }
            .detail-value {
              color: #666;
            }
            @media print {
              body { margin: 0; }
              .qr-container { border: 1px solid #333; }
              .qr-code img { 
                border: 1px solid #333;
                print-color-adjust: exact;
                -webkit-print-color-adjust: exact;
              }
            }
          </style>
        </head>
        <body>
          <div class="qr-container">
            <div class="qr-title">UC Davis Harvest</div>
            <div class="project-info">Field Request QR Code</div>
            
            <div class="project-details">
              <div class="detail-row">
                <span class="detail-label">Project ID:</span>
                <span class="detail-value">#${project.id}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Project Name:</span>
                <span class="detail-value">${project.name.replace(
                  /"/g,
                  "&quot;"
                )}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Principal Investigator:</span>
                <span class="detail-value">${(
                  project.principalInvestigator?.name || "N/A"
                ).replace(/"/g, "&quot;")}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Team:</span>
                <span class="detail-value">${team}</span>
              </div>
            </div>
            
            <div class="qr-code">
              <img src="${qrDataUrl}" alt="QR Code for Project ${
        project.id
      }" width="256" height="256" />
            </div>
            
            <div class="instructions">
              <strong>Scan this QR code with your mobile device to quickly access this project.</strong><br>
              This code links directly to the project details page in the UC Davis Harvest system.
            </div>
          </div>
        </body>
        </html>
      `;

      printWindow.document.write(printContent);
      printWindow.document.close();

      // Wait a moment for the content to load before printing
      setTimeout(() => {
        printWindow.focus();
        printWindow.print();
        printWindow.close();
      }, 500);
    }
  };

  return (
    <div className="modal fade show" style={{ display: "block" }} tabIndex={-1}>
      <div className="modal-dialog modal-lg">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">Project QR Code</h5>
            <button
              type="button"
              className="close"
              onClick={onClose}
              aria-label="Close"
            >
              <span aria-hidden="true">&times;</span>
            </button>
          </div>
          <div className="modal-body text-center">
            <div className="mb-3">
              <h6>Field Request #{project.id}</h6>
              <p className="text-muted">{project.name}</p>
            </div>

            {qrCodeGenerated ? (
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
          </div>
          <div className="modal-footer">
            <button
              type="button"
              className="btn btn-primary"
              onClick={handlePrint}
              disabled={!qrCodeGenerated || !qrDataUrl}
            >
              Print QR Code
            </button>
            <button
              type="button"
              className="btn btn-secondary"
              onClick={onClose}
            >
              Close
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
