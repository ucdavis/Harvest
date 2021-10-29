import React from "react";

import { PDFDownloadLink } from "@react-pdf/renderer";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDownload } from "@fortawesome/free-solid-svg-icons";

import { InvoicePDF } from "./InvoicePDF";
import { Invoice } from "../types";

interface Props {
  invoice: Invoice;
  fileName: string;
}

const InvoicePdfLink = (props: Props) => (
  <PDFDownloadLink
    document={<InvoicePDF invoice={props.invoice} />}
    fileName={props.fileName}
  >
    <button className="btn btn-link">
      Download PDF <FontAwesomeIcon icon={faDownload} />
    </button>
  </PDFDownloadLink>
);

export default InvoicePdfLink;
