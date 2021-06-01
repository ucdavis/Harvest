import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { ProjectAccount, ProjectWithInvoice } from "../types";
import { AccountsInput } from "../Requests/AccountsInput";
import { ProjectHeader } from "../Requests/ProjectHeader";
import { InvoiceDisplay } from "./InvoiceDisplay";
import { formatCurrency } from "../Util/NumberFormatting";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDownload } from "@fortawesome/free-solid-svg-icons";

interface RouteParams {
  invoiceId?: string;
}

export const InvoiceDetailContainer = () => {
  const { invoiceId } = useParams<RouteParams>();
  const [projectAndInvoice, setProjectAndInvoice] = useState<ProjectWithInvoice>();
  const [accounts, setAccounts] = useState<ProjectAccount[]>([]); // TODO: better to have part of project obj?

  useEffect(() => {
    const cb = async () => {
      const invoiceResponse = await fetch(`/Invoice/Get/${invoiceId}`);

      if (invoiceResponse.ok) {
        const projectWithInvoice: ProjectWithInvoice = await invoiceResponse.json();

        setProjectAndInvoice(projectWithInvoice);
      }
    };

    cb();
  }, [invoiceId]);

  if (projectAndInvoice === undefined) {
    return <div>Loading ...</div>;
  }

  if (
    projectAndInvoice.project === undefined ||
    projectAndInvoice.invoice === undefined ||
    projectAndInvoice.invoice === null
  ) {
    return <div>No project or invoice found</div>;
  }

  return (
    <div className="card-wrapper">
      <ProjectHeader project={projectAndInvoice.project} title={"Invoice #" + invoiceId + " - Project #" + projectAndInvoice.project.id}></ProjectHeader>
      <div className="card-green-bg">
        <div className="card-content">
          <InvoiceDisplay invoice={projectAndInvoice.invoice}></InvoiceDisplay>
        </div>
      </div>
    </div>
  );
};
