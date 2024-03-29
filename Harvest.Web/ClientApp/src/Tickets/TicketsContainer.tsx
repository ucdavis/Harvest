import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { CommonRouteParams, Project, Ticket } from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { TicketTable } from "./TicketTable";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";

export const TicketsContainer = () => {
  const { projectId, team } = useParams<CommonRouteParams>();
  const [project, setProject] = useState<Project>();
  const [tickets, setTickets] = useState<Ticket[]>([]);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/Project/Get/${projectId}`
      );

      if (response.ok) {
        const proj: Project = await response.json();
        getIsMounted() && setProject(proj);
      }
    };

    cb();
  }, [projectId, getIsMounted, team]);

  useEffect(() => {
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/Ticket/GetList?projectId=${projectId}`
      );

      if (response.ok) {
        getIsMounted() && setTickets(await response.json());
      }
    };

    cb();
  }, [projectId, getIsMounted, team]);

  if (project === undefined || tickets === undefined) {
    return <div>Loading...</div>;
  }

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project.id || "")}
      ></ProjectHeader>
      <div id="ticketTableContainer" className="card-content">
        <div className="row justify-content-between">
          <h3>List of all tickets for your project</h3>

          <Link to={`/${team}/ticket/create/${projectId}`}>Create Ticket</Link>
        </div>
        <div className="row justify-content-center">
          <TicketTable compact={false} tickets={tickets} />
        </div>
      </div>
    </div>
  );
};
