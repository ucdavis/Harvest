import React, { Suspense, useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { ProjectWithInvoice } from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { InvoiceDisplay } from "./InvoiceDisplay";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";

// Lazy load quote pdf link since it's a large JS file and causes a console warning
const InvoicePDFLink = React.lazy(() => import("../Pdf/InvoicePDFLink"));

interface RouteParams {
  projectId: string;
  invoiceId: string;
}

export const InvoiceDetailContainer = () => {
  const { projectId, invoiceId } = useParams<RouteParams>();
  const [projectAndInvoice, setProjectAndInvoice] =
    useState<ProjectWithInvoice>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const invoiceResponse = await authenticatedFetch(
        `/api/Invoice/Get/${projectId}?invoiceId=${invoiceId}`
      );

      if (invoiceResponse.ok) {
        const projectWithInvoice: ProjectWithInvoice =
          await invoiceResponse.json();

        getIsMounted() && setProjectAndInvoice(projectWithInvoice);
      }
    };

    cb();
  }, [invoiceId, getIsMounted, projectId]);

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
          <div className="row justify-content-center pt-2 pb-2">
            <InvoicePDFLink
              invoice={projectAndInvoice.invoice}
              fileName={`Invoice-${invoiceId}-Project-${projectAndInvoice.project.name}.pdf`}
            ></InvoicePDFLink>
          </div>
        </Suspense>
      </div>
    </div>
  );
};
