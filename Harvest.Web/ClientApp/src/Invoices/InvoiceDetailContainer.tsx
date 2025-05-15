import React, { Suspense, useContext, useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { ProjectWithInvoice, CommonRouteParams } from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { InvoiceDisplay } from "./InvoiceDisplay";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";
import AppContext from "../Shared/AppContext";

// Lazy load quote pdf link since it's a large JS file and causes a console warning
const InvoicePDFLink = React.lazy(() => import("../Pdf/InvoicePDFLink"));

interface RouteParams {
  projectId: string;
  invoiceId: string;
  shareId?: string;
}

export const InvoiceDetailContainer = () => {
  const { projectId, invoiceId, shareId } = useParams<RouteParams>();
  const { team } = useParams<CommonRouteParams>();
  const [projectAndInvoice, setProjectAndInvoice] =
    useState<ProjectWithInvoice>();
  const userInfo = useContext(AppContext);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    if (
      shareId &&
      !userInfo.user.roles.includes("Shared") &&
      !userInfo.user.roles.includes("PI")
    ) {
      userInfo.user.roles.push("Shared");
      console.log("User Roles: ", userInfo.user.roles);
    }

    const cb = async () => {
      const invoiceResponse = await authenticatedFetch(
        `/api/${team}/Invoice/Get/${projectId}?invoiceId=${invoiceId}&shareId=${shareId}`
      );

      if (invoiceResponse.ok) {
        const projectWithInvoice: ProjectWithInvoice =
          await invoiceResponse.json();

        getIsMounted() && setProjectAndInvoice(projectWithInvoice);
      }
    };

    cb();
  }, [invoiceId, getIsMounted, projectId, team, shareId, userInfo.user.roles]);

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
