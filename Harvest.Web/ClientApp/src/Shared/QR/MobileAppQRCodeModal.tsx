import React from "react";
import { useQRCodeGeneration } from "./useQRCodeGeneration";

export interface MobileAppQRCodeModalProps {
  onClose: () => void;
}

export const MobileAppQRCodeModal: React.FC<MobileAppQRCodeModalProps> = ({
  onClose,
}) => {
  const appStoreUrl = "https://apps.apple.com/app/id6752299223";

  const { canvasRef, qrCodeGenerated, error } = useQRCodeGeneration(
    appStoreUrl,
    {
      width: 300,
      margin: 2,
    }
  );

  return (
    <div className="modal fade show" style={{ display: "block" }} tabIndex={-1}>
      <div className="modal-dialog modal-lg">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">Download Harvest Mobile App</h5>
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
              <h4>Harvest Mobile iPhone App</h4>
              <p className="text-muted">
                Scan this QR code with your iPhone camera to download the
                Harvest mobile app
              </p>
            </div>

            {error ? (
              <div className="alert alert-danger">
                Error generating QR code: {error}
              </div>
            ) : (
              <div className="d-flex justify-content-center mb-3">
                <canvas
                  ref={canvasRef}
                  style={{
                    border: "1px solid #ddd",
                    borderRadius: "8px",
                    maxWidth: "100%",
                    height: "auto",
                  }}
                />
              </div>
            )}

            {qrCodeGenerated && (
              <div className="mt-3 text-center">
                <p className="mb-1">Or visit directly:</p>
                <a
                  href={appStoreUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="d-block"
                >
                  {appStoreUrl}
                </a>
              </div>
            )}
          </div>
          <div className="modal-footer">
            <button type="button" className="btn btn-danger" onClick={onClose}>
              Close
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
