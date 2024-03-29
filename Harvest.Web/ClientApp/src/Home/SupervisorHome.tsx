import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { Project, Ticket, CommonRouteParams } from "../types";
import { authenticatedFetch } from "../Util/Api";
import { StatusToActionRequired } from "../Util/MessageHelpers";
import { useIsMounted } from "../Shared/UseIsMounted";

export const SupervisorHome = () => {
  const [projects, setProjects] = useState<Project[]>([]);
  const [tickets, setTickets] = useState<Ticket[]>([]);

  const { team } = useParams<CommonRouteParams>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get info on projects requiring approval
    const getProjects = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/project/RequiringManagerAttention`
      );
      if (getIsMounted()) {
        const projects: Project[] = await response.json();
        getIsMounted() && setProjects(projects);
      }
    };

    getProjects();
  }, [getIsMounted, team]);

  useEffect(() => {
    const getTicketsWaitingForMe = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/ticket/RequiringManagerAttention?limit=3`
      );
      if (getIsMounted()) {
        const tickets: Ticket[] = await response.json();
        getIsMounted() && setTickets(tickets);
      }
    };

    getTicketsWaitingForMe();
  }, [getIsMounted, team]);

  return (
    <>
      <h5>Quick Actions</h5>
      <ul className="list-group quick-actions">
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
