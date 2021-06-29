import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { Project, Ticket } from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { TicketTable } from "./TicketTable";

interface RouteParams {
  projectId?: string;
}

export const AllTickets = () => {
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
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch(
        `/Ticket/List?projectId=${projectId}&topOnly=false`
      );

      if (response.ok) {
        setTickets(await response.json());
      }
    };

    cb();
  });

  if (project === undefined || tickets === undefined) {
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
        <TicketTable compact={false} tickets={tickets}></TicketTable>
      </div>
    </div>
  );
};
