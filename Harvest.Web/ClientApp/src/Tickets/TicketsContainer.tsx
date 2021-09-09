import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { Project, Ticket } from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { TicketTable } from "./TicketTable";
import { useIsMounted } from "../Shared/UseIsMounted";

import { ReturnToProject } from "../Shared/ReturnToProject";

interface RouteParams {
  projectId?: string;
}

export const TicketsContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();
  const [tickets, setTickets] = useState<Ticket[]>([]);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const response = await fetch(`/Project/Get/${projectId}`);

      if (response.ok) {
        const proj: Project = await response.json();
        getIsMounted() && setProject(proj);
      }
    };

    cb();
  }, [projectId, getIsMounted]);

  useEffect(() => {
    const cb = async () => {
      const response = await fetch(`/Ticket/GetList?projectId=${projectId}`);

      if (response.ok) {
        getIsMounted() && setTickets(await response.json());
      }
    };

    cb();
  }, [projectId, getIsMounted]);

  if (project === undefined || tickets === undefined) {
    return <div>Loading...</div>;
  }

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project.id || "")}
      ></ProjectHeader>
      <ReturnToProject projectId={projectId} />
      <div className="card-content">
        <div className="row justify-content-between">
          <h3>List of all tickets for your project</h3>

          <Link to={`/ticket/create/${projectId}`}>Create Ticket</Link>
        </div>
        <div className="row justify-content-center">
          <TicketTable compact={false} tickets={tickets} />
        </div>
      </div>
    </div>
  );
};
