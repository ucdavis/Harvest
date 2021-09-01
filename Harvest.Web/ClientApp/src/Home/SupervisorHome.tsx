import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Project } from "../types";
import { StatusToActionRequired } from "../Util/MessageHelpers";
import { useIsMounted } from "../Shared/UseIsMounted";

export const SupervisorHome = () => {
  const [projects, setProjects] = useState<Project[]>();

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
      </ul>
    </>
  );
};
