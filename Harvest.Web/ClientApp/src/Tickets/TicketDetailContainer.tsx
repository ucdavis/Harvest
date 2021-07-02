import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { Project, Ticket, TicketAttachment, TicketMessage, TicketDetails} from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";

interface RouteParams {
    projectId: string;
    ticketId: string;
}

export const TicketDetailContainer = () => {
  const { projectId, ticketId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();
  const [ticket, setTicket] = useState<TicketDetails>();
    const [attachment, setAttachment] = useState<TicketAttachment>();
    const [message, setMessage] = useState<TicketMessage>();

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
          const response = await fetch(`/Ticket/Get/${projectId}/${ticketId}`);

          if (response.ok) {
              const tick: TicketDetails = await response.json();
              setTicket(tick);
          }
      };

      cb();
  }, [ticketId, projectId]);



  if (project === undefined || ticket === undefined) {
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
              
              <p>{ticket.name}</p>
              <p>{ticket.attachments[0].fileName}</p>
          <p>{ticket.messages[0].message}</p>
      </div>
    </div>
  );
};
