import React from "react";

export interface QRCodeModalProps {
  title: string;
  itemName: string;
  itemId: string | number;
  children: React.ReactNode;
  onClose: () => void;
  onPrint: () => void;
  qrCodeGenerated: boolean;
  qrDataUrl: string;
}

export const QRCodeModal: React.FC<QRCodeModalProps> = ({
  title,
  itemName,
  itemId,
  children,
  onClose,
  onPrint,
  qrCodeGenerated,
  qrDataUrl,
}) => {
  return (
    <div className="modal fade show" style={{ display: "block" }} tabIndex={-1}>
      <div className="modal-dialog modal-lg">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">{title}</h5>
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
              <h6>#{itemId}</h6>
              <p className="text-muted">{itemName}</p>
            </div>

            {children}
          </div>
          <div className="modal-footer">
            <button
              type="button"
              className="btn btn-primary"
              onClick={onPrint}
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
