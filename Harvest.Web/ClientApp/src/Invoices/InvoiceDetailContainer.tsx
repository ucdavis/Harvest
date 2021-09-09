import React, { Suspense, useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { ProjectWithInvoice } from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { InvoiceDisplay } from "./InvoiceDisplay";
import { useIsMounted } from "../Shared/UseIsMounted";

// Lazy load quote pdf link since it's a large JS file and causes a console warning
const InvoicePDFLink = React.lazy(() => import("../Pdf/InvoicePDFLink"));

interface RouteParams {
  invoiceId?: string;
}

export const InvoiceDetailContainer = () => {
  const { invoiceId } = useParams<RouteParams>();
  const [projectAndInvoice, setProjectAndInvoice] =
    useState<ProjectWithInvoice>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const invoiceResponse = await fetch(`/Invoice/Get/${invoiceId}`);

      if (invoiceResponse.ok) {
        const projectWithInvoice: ProjectWithInvoice =
          await invoiceResponse.json();

        getIsMounted() && setProjectAndInvoice(projectWithInvoice);
      }
    };

    cb();
  }, [invoiceId, getIsMounted]);

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
        title={`Invoice #${invoiceId} (${projectAndInvoice.invoice.status}) - Project #${projectAndInvoice.project.id}`}
      />
      <div className="card-green-bg">
        <div className="card-content">
          <InvoiceDisplay invoice={projectAndInvoice.invoice}></InvoiceDisplay>
        </div>
        <Suspense fallback={<div>Generating PDF ...</div>}>
          <InvoicePDFLink
            invoice={projectAndInvoice.invoice}
            fileName={`Invoice-${invoiceId}-Project-${projectAndInvoice.project.name}.pdf`}
          ></InvoicePDFLink>
        </Suspense>
      </div>
    </div>
  );
};
