import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Project, Ticket } from "../types";
import { StatusToActionRequired } from "../Util/MessageHelpers";
import { useIsMounted } from "../Shared/UseIsMounted";

export const PIHome = () => {
  const [projects, setProjects] = useState<Project[]>([]);
  const [tickets, setTickets] = useState<Ticket[]>([]);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const getProjectsWaitingForMe = async () => {
      const response = await fetch("/project/RequiringPIAttention");
      if (getIsMounted()) {
        const projects: Project[] = await response.json();
        getIsMounted() && setProjects(projects);
      }
    };

    getProjectsWaitingForMe();
  }, [getIsMounted]);

  useEffect(() => {
    const getTicketsWaitingForMe = async () => {
      const response = await fetch("/ticket/RequiringPIAttention?limit=3");
      if (getIsMounted()) {
        const tickets: Ticket[] = await response.json();
        getIsMounted() && setTickets(tickets);
      }
    };

    getTicketsWaitingForMe();
  }, [getIsMounted]);

  return (
    <>
      <h5>Quick Actions</h5>
      <ul className="list-group quick-actions">
        <li className="list-group-item">
          <Link to="/project/mine">View My Projects</Link>
        </li>
        <li className="list-group-item">
          <Link to="/request/create">Request New Project</Link>
        </li>
        {projects.map((project) => (
          <li key={project.id} className="list-group-item">
            <Link to={`/project/details/${project.id}`}>
              View project {project.name}{" "}
              <span
                className={`badge badge-primary badge-status-${project.status}`}
              >
                {StatusToActionRequired(project.status)}
              </span>
            </Link>
          </li>
        ))}
      </ul>
      {tickets.length > 0 && (
        <>
          <br />
          <h5>Tickets</h5>
          <ul className="list-group quick-actions">
            <li className="list-group-item">
              <Link to="/ticket/mine">View My Open Tickets</Link>
            </li>
            {tickets.map((ticket) => (
              <li key={ticket.id} className="list-group-item">
                <Link to={`/ticket/details/${ticket.projectId}/${ticket.id}`}>
                  View ticket: "{ticket.name}"
                </Link>
              </li>
            ))}
          </ul>
        </>
      )}
      <br />
    </>
  );
};
