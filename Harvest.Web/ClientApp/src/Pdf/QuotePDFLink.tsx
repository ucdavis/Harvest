import React from "react";

import { PDFDownloadLink } from "@react-pdf/renderer";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDownload } from "@fortawesome/free-solid-svg-icons";

import { QuotePDF } from "./QuotePDF";
import { QuoteContent } from "../types";

interface Props {
  quote: QuoteContent;
  fileName: string;
}

const QuotePdfLink = (props: Props) => (
  <PDFDownloadLink
    document={<QuotePDF quote={props.quote} />}
    fileName={props.fileName}
  >
    <button className="btn btn-link btn-sm">
      Download PDF <FontAwesomeIcon icon={faDownload} />
    </button>
  </PDFDownloadLink>
);

export default QuotePdfLink;
