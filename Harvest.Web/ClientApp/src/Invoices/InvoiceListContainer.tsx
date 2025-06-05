import React, { useContext, useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { Invoice, Project, CommonRouteParams } from "../types";
import { InvoiceTable } from "./InvoiceTable";
import { useIsMounted } from "../Shared/UseIsMounted";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { authenticatedFetch } from "../Util/Api";
import AppContext from "../Shared/AppContext";

interface RouteParams {
  projectId?: string;
}

export const InvoiceListContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const { team, shareId } = useParams<CommonRouteParams>();
  const [project, setProject] = useState<Project>();
  const [invoices, setInvoices] = useState<Invoice[]>([]);
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
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/Invoice/List/?projectId=${projectId}&shareId=${shareId}`
      );

      if (response.ok) {
        getIsMounted() && setInvoices(await response.json());
      }
    };

    if (projectId) {
      cb();
    }
  }, [projectId, getIsMounted, team, shareId, userInfo.user.roles]);
  useEffect(() => {
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/Project/Get/${projectId}/${shareId}`
      );

      if (response.ok) {
        const proj: Project = await response.json();
        getIsMounted() && setProject(proj);
      }
    };

    cb();
  }, [projectId, getIsMounted, team, shareId]);

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
        <InvoiceTable invoices={invoices} shareId={shareId}></InvoiceTable>
      </div>
    </div>
  );
};
