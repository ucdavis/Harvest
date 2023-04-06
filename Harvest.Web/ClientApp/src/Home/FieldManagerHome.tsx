import React, { useEffect, useState } from "react";
import { Link, Redirect } from "react-router-dom";

import { authenticatedFetch } from "../Util/Api";
import { StatusToActionRequired } from "../Util/MessageHelpers";
import { CommonRouteParams, Project, Ticket } from "../types";
import { useIsMounted } from "../Shared/UseIsMounted";
import { useParams } from "react-router";

export const FieldManagerHome = () => {
  const [projects, setProjects] = useState<Project[]>([]);
  const [tickets, setTickets] = useState<Ticket[]>([]);

  const { team } = useParams<CommonRouteParams>();

  const getIsMounted = useIsMounted();

  useEffect(() => {
    // get info on projects requiring approval
    const getProjects = async () => {
      const response = await authenticatedFetch(
        `/api/project/RequiringManagerAttention?team=${team}`
      );
      if (getIsMounted()) {
        const projects: Project[] = await response.json();
        getIsMounted() && setProjects(projects);
      }
    };

    getProjects();
  }, [getIsMounted]);

  useEffect(() => {
    const getTicketsWaitingForMe = async () => {
      const response = await authenticatedFetch(
        `/api/ticket/RequiringManagerAttention?team=${team}&limit=3`
      );
      if (getIsMounted()) {
        const tickets: Ticket[] = await response.json();
        getIsMounted() && setTickets(tickets);
      }
    };

    getTicketsWaitingForMe();
  }, [getIsMounted]);

  if (!team) {
    return <Redirect to="/team" />;
  }

  return (
    <>
      <h5>Quick Actions</h5>
      <ul className="list-group quick-actions">
        {projects !== undefined && projects.length > 0 && (
          <li className="list-group-item">
            <Link to={`/${team}/project/needsAttention`}>
              View Projects Requiring Attention{" "}
              <span className="badge badge-pill badge-primary">
                {projects.length > 3 ? "3+" : projects.length}
              </span>
            </Link>
          </li>
        )}
        <li className="list-group-item">
          <Link to={`/${team}/project`}>View All Projects</Link>
        </li>
        <li className="list-group-item">
          <Link to={`/${team}/expense/entry`}>Enter Project Expenses</Link>
        </li>
        {projects.slice(0, 3).map((project) => (
          <li key={project.id} className="list-group-item">
            <Link to={`/${project.team.slug}/project/details/${project.id}`}>
              Quick jump to {project.name}{" "}
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
              <Link to={`/${team}/ticket/needsAttention`}>
                View Open Tickets
              </Link>
            </li>
            {tickets.map((ticket) => (
              <li key={ticket.id} className="list-group-item">
                <Link
                  to={`/${ticket.project.team.slug}/ticket/details/${ticket.projectId}/${ticket.id}`}
                >
                  View ticket: "{ticket.name}"
                </Link>
              </li>
            ))}
          </ul>
        </>
      )}
    </>
  );
};
