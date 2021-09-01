import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Project } from "../types";
import { StatusToActionRequired } from "../Util/MessageHelpers";
import { useIsMounted } from "../Shared/UseIsMounted";

export const PIHome = () => {
  const [projects, setProjects] = useState<Project[]>([]);

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
              <span className="badge badge-light">
                {StatusToActionRequired(project.status)}
              </span>
            </Link>
          </li>
        ))}
      </ul>
    </>
  );
};
