import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { Invoice, Project } from "../types";
import { InvoiceTable } from "./InvoiceTable";
import { useIsMounted } from "../Shared/UseIsMounted";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { authenticatedFetch } from "../Util/Api";

interface RouteParams {
  projectId?: string;
}

export const InvoiceListContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();
  const [invoices, setInvoices] = useState<Invoice[]>([]);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/Invoice/List/?projectId=${projectId}`
      );

      if (response.ok) {
        getIsMounted() && setInvoices(await response.json());
      }
    };

    if (projectId) {
      cb();
    }
  }, [projectId, getIsMounted]);
  useEffect(() => {
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/Project/Get/${projectId}`
      );

      if (response.ok) {
        const proj: Project = await response.json();
        getIsMounted() && setProject(proj);
      }
    };

    cb();
  }, [projectId, getIsMounted]);

  if (project === undefined) {
    return <div>Loading...</div>;
  }

  if (invoices.length === 0) {
    return <div>No invoices found</div>;
  }

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project.id || "")}
      ></ProjectHeader>
      <div className="card-content">
        <h3>Invoices</h3>
        <InvoiceTable
          invoices={invoices}
          team={project.team?.slug}
        ></InvoiceTable>
      </div>
    </div>
  );
};
