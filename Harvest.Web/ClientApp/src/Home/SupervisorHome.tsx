import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Project, Ticket } from "../types";
import { StatusToActionRequired } from "../Util/MessageHelpers";
import { useIsMounted } from "../Shared/UseIsMounted";

export const SupervisorHome = () => {
  const [projects, setProjects] = useState<Project[]>();
  const [tickets, setTickets] = useState<Ticket[]>([]);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get info on projects requiring approval
    const getProjects = async () => {
      const response = await fetch("/project/RequiringManagerAttention");
      if (getIsMounted()) {
        const projects: Project[] = await response.json();
        getIsMounted() && setProjects(projects);
      }
    };

    getProjects();
  }, [getIsMounted]);

  useEffect(() => {
    const getTicketsWaitingForMe = async () => {
      const response = await fetch("/ticket/RequiringManagerAttention");
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
          <Link to="/project">View Projects</Link>
        </li>
        <li className="list-group-item">
          <Link to="/expense/entry">Enter Expenses</Link>
        </li>
        {projects !== undefined && projects.length > 0 && (
          <li className="list-group-item">
            <Link to={`/project/details/${projects[0].id}`}>
              Quick jump to {projects[0].name}{" "}
              <span className="badge badge-primary">
                {StatusToActionRequired(projects[0].status)}
              </span>
            </Link>
          </li>
        )}
        {projects !== undefined && projects.length > 1 && (
          <li className="list-group-item">
            <Link to={`/project/details/${projects[1].id}`}>
              Quick jump to {projects[1].name}{" "}
              <span className="badge badge-primary">
                {StatusToActionRequired(projects[1].status)}
              </span>
            </Link>
          </li>
        )}
        {tickets.map((ticket) => (
          <li key={ticket.id} className="list-group-item">
            <Link to={`/ticket/details/${ticket.projectId}/${ticket.id}`}>
              View ticket: "{ticket.name}"
            </Link>
          </li>
        ))}
      </ul>
    </>
  );
};
