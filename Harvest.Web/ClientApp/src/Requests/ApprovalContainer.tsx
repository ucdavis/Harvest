import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import {
  PDFDownloadLink,
  Page,
  Text,
  View,
  Document,
  StyleSheet,
} from "@react-pdf/renderer";
import {
  Table,
  TableHeader,
  TableCell,
  TableBody,
  DataTableCell,
} from "@david.kucsai/react-pdf-table";

import { ActivityRateTypes } from "../constants";
import { ProjectAccount, ProjectWithQuote } from "../types";
import { AccountsInput } from "./AccountsInput";
import { ProjectHeader } from "./ProjectHeader";
import { QuoteDisplay } from "../Quotes/QuoteDisplay";
import { formatCurrency } from "../Util/NumberFormatting";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDownload } from "@fortawesome/free-solid-svg-icons";

interface RouteParams {
  projectId?: string;
}

export const ApprovalContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [projectAndQuote, setProjectAndQuote] = useState<ProjectWithQuote>();
  const [accounts, setAccounts] = useState<ProjectAccount[]>([]); // TODO: better to have part of project obj?
  const [disabled, setDisabled] = useState<boolean>(true);

  useEffect(() => {
    const cb = async () => {
      const quoteResponse = await fetch(`/Quote/Get/${projectId}`);

      if (quoteResponse.ok) {
        const projectWithQuote: ProjectWithQuote = await quoteResponse.json();

        setProjectAndQuote(projectWithQuote);
      }
    };

    cb();
  }, [projectId]);

  const approve = async () => {
    const model = { accounts };
    // TODO: validation, loading spinner
    const response = await fetch(`/Request/Approve/${projectId}`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(model),
    });

    if (response.ok) {
      window.location.pathname = `/Project/Details/${projectId}`;
    } else {
      alert("Something went wrong, please try again");
    }
  };

  if (projectAndQuote === undefined) {
    return <div>Loading ...</div>;
  }

  // TODO: make sure both project and quote are in the correct statuses in order for an approval
  if (
    projectAndQuote.project === undefined ||
    projectAndQuote.quote === undefined ||
    projectAndQuote.quote === null
  ) {
    return <div>No project or open quote found</div>;
  }

  const styles = StyleSheet.create({
    page: {
      flexDirection: "row",
      backgroundColor: "#E4E4E4",
    },
    section: {
      margin: 10,
      padding: 10,
      flexGrow: 1,
    },
  });

  const MyDocument = () => (
    <Document>
      <Page size="A4" style={styles.page}>
        <View style={styles.section}>
          <Text>Quote</Text>
          <Text>
            {" "}
            {projectAndQuote.quote?.acreageRateDescription}:{" "}
            {projectAndQuote.quote?.acres} @{" "}
            {projectAndQuote.quote != undefined
              ? formatCurrency(projectAndQuote.quote.acreageRate)
              : 0}{" "}
            = $
            {projectAndQuote.quote != undefined
              ? formatCurrency(projectAndQuote.quote.acreageTotal)
              : 0}
          </Text>
        </View>
    
      </Page>
    </Document>
  );

  // we have a project with a quote, time for the approval step
  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={projectAndQuote.project}
        title={"Field Request #" + (projectAndQuote.project.id || "")}
      ></ProjectHeader>
      <div className="card-green-bg">
        <div className="card-content">
          <QuoteDisplay quote={projectAndQuote.quote}></QuoteDisplay>
          <div className="row">
            <div className="col-md-6">
              <h2 className="primary-font bold-font">
                Quote Total: ${formatCurrency(projectAndQuote.quote.grandTotal)}
              </h2>
              <PDFDownloadLink
                document={<MyDocument />}
                fileName="somename.pdf"
              >
                <button className="btn btn-link btn-sm">
                  Download PDF <FontAwesomeIcon icon={faDownload} />
                </button>
              </PDFDownloadLink>
              <AccountsInput
                accounts={accounts}
                setAccounts={setAccounts}
                setDisabled={setDisabled}
              />
            </div>
            <div className="col-md-6">
              <p>
                <b>Terms and Conditions</b>
              </p>
              <ol>
                <li>
                  This estimate is approximate based on the information provided
                  by the client. A change order is required if substantial
                  elements of the project are altered. Estimate valid for 30
                  days.
                </li>
                <li>
                  Payment for initial materials/supplies due prior to work
                  performed.
                </li>
                <li>
                  Payment for work performed due immediately upon completion of
                  job.
                </li>
              </ol>
              <div className="text-right mt-5">
                <button className="btn btn-link mr-2">Reject</button>
                <button
                  className="btn btn-primary"
                  disabled={disabled}
                  onClick={approve}
                >
                  Approve Quote
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
