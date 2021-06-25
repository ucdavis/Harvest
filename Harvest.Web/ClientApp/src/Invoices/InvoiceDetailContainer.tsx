import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { ProjectWithInvoice } from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { InvoiceDisplay } from "./InvoiceDisplay";

interface RouteParams {
  invoiceId?: string;
}

export const InvoiceDetailContainer = () => {
  const { invoiceId } = useParams<RouteParams>();
  const [
    projectAndInvoice,
    setProjectAndInvoice,
  ] = useState<ProjectWithInvoice>();

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
      <ProjectHeader
        project={projectAndInvoice.project}
        title={
          "Invoice #" +
          invoiceId +
          " - Project #" +
          projectAndInvoice.project.id
        }
      ></ProjectHeader>
      <div className="card-green-bg">
        <div className="card-content">
          <InvoiceDisplay invoice={projectAndInvoice.invoice}></InvoiceDisplay>
        </div>
      </div>
    </div>
  );
};
