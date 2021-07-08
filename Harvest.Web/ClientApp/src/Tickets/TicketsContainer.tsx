import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { Project, Ticket } from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { TicketTable } from "./TicketTable";

interface RouteParams {
  projectId?: string;
}

export const TicketsContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();
  const [tickets, setTickets] = useState<Ticket[]>([]);

  useEffect(() => {
    const cb = async () => {
      const response = await fetch(`/Project/Get/${projectId}`);

      if (response.ok) {
        const proj: Project = await response.json();
        setProject(proj);
      }
    };

    cb();
  }, [projectId]);

  useEffect(() => {
    const cb = async () => {
      const response = await fetch(`/Ticket/GetList?projectId=${projectId}`);

      if (response.ok) {
        setTickets(await response.json());
      }
    };

    cb();
  }, [projectId]);

  if (project === undefined) {
    return <div>Loading...</div>;
  }

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project.id || "")}
      ></ProjectHeader>
      <div className="card-content">
        <div className="card-head">
          <h2>List of all tickets for your project</h2>
        </div>
        <Link
          className="btn btn-primary btn-small mr-4"
          to={`/ticket/create/${projectId}`}
        >
          Create Ticket
        </Link>
        <TicketTable compact={false} tickets={tickets} />
      </div>
    </div>
  );
};
