export interface PrintDetail {
  label: string;
  value: string;
}

export interface QRCodePrintOptions {
  title: string;
  subtitle: string;
  itemId: string | number;
  itemName: string;
  details: PrintDetail[];
  qrDataUrl: string;
  instructions?: string;
}

export interface RateIdCardPrintOptions {
  teamName: string;
  rateName: string;
  qrDataUrl: string;
}

export const generateQRCodePrintHTML = (
  options: QRCodePrintOptions
): string => {
  const {
    title,
    subtitle,
    itemId,
    itemName,
    details,
    qrDataUrl,
    instructions = "Scan this QR code with your mobile device to quickly access this item.",
  } = options;

  const detailRows = details
    .map(
      (detail) => `
        <div class="detail-row">
          <span class="detail-label">${detail.label}:</span>
          <span class="detail-value">${detail.value.replace(
            /"/g,
            "&quot;"
          )}</span>
        </div>
      `
    )
    .join("");

  return `
    <!DOCTYPE html>
    <html>
    <head>
      <title>${title} - ${itemName}</title>
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
        .qr-subtitle {
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
        .item-details {
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
        <div class="qr-title">${title}</div>
        <div class="qr-subtitle">${subtitle}</div>
        
        <div class="item-details">
          <div class="detail-row">
            <span class="detail-label">ID:</span>
            <span class="detail-value">#${itemId}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Name:</span>
            <span class="detail-value">${itemName.replace(
              /"/g,
              "&quot;"
            )}</span>
          </div>
          ${detailRows}
        </div>
        
        <div class="qr-code">
          <img src="${qrDataUrl}" alt="QR Code for ${itemId}" width="256" height="256" />
        </div>
        
        <div class="instructions">
          <strong>${instructions}</strong><br>
          This code links directly to the details page in the UC Davis Harvest system.
        </div>
      </div>
    </body>
    </html>
  `;
};

export const printQRCode = (printOptions: QRCodePrintOptions): void => {
  const printContent = generateQRCodePrintHTML(printOptions);
  const printWindow = window.open("", "_blank");

  if (printWindow) {
    printWindow.document.documentElement.innerHTML = printContent;

    // Wait a moment for the content to load before printing
    setTimeout(() => {
      printWindow.focus();
      printWindow.print();
      printWindow.close();
    }, 500);
  }
};

export const generateRateIdCardPrintHTML = (
  options: RateIdCardPrintOptions
): string => {
  const { teamName, rateName, qrDataUrl } = options;

  return `
    <!DOCTYPE html>
    <html>
    <head>
      <title>Rate ID Card - ${rateName.replace(/"/g, "&quot;")}</title>
      <style>
        @page {
          size: 3.375in 2.125in;
          margin: 0;
        }
        body {
          font-family: Arial, sans-serif;
          margin: 0;
          padding: 0;
          width: 3.375in;
          height: 2.125in;
        }
        .card {
          box-sizing: border-box;
          width: 100%;
          height: 100%;
          padding: 0.2in 0.18in;
          display: grid;
          grid-template-columns: 1fr 1.05in;
          column-gap: 0.18in;
          align-items: center;
          border: 1px solid #222;
          border-radius: 0.06in;
        }
        .meta {
          display: flex;
          flex-direction: column;
          gap: 0.1in;
        }
        .team {
          font-size: 0.22in;
          font-weight: bold;
          color: #111;
        }
        .rate {
          font-size: 0.18in;
          line-height: 1.2;
          color: #333;
        }
        .qr {
          display: flex;
          align-items: center;
          justify-content: center;
        }
        .qr img {
          width: 1.05in;
          height: 1.05in;
          border: 1px solid #222;
        }
        @media print {
          body { print-color-adjust: exact; -webkit-print-color-adjust: exact; }
          .qr img { print-color-adjust: exact; -webkit-print-color-adjust: exact; }
        }
      </style>
    </head>
    <body>
      <div class="card">
        <div class="meta">
          <div class="team">${teamName.replace(/"/g, "&quot;")}</div>
          <div class="rate">${rateName.replace(/"/g, "&quot;")}</div>
        </div>
        <div class="qr">
          <img src="${qrDataUrl}" alt="QR Code for ${rateName.replace(
    /"/g,
    "&quot;"
  )}" />
        </div>
      </div>
    </body>
    </html>
  `;
};

export const printRateIdCard = (options: RateIdCardPrintOptions): void => {
  const printContent = generateRateIdCardPrintHTML(options);
  const printWindow = window.open("", "_blank");

  if (printWindow) {
    printWindow.document.documentElement.innerHTML = printContent;

    setTimeout(() => {
      printWindow.focus();
      printWindow.print();
      printWindow.close();
    }, 500);
  }
};
