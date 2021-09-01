import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Project } from "../types";
import { useIsMounted } from "../Shared/UseIsMounted";

export const WorkerHome = () => {
  const [projects, setProjects] = useState<Project[]>([]);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const getProjectsWithRecentExpenses = async () => {
      const response = await fetch("/expense/GetRecentExpensedProjects");
      if (getIsMounted()) {
        const projects: Project[] = await response.json();
        getIsMounted() && setProjects(projects);
      }
    };

    getProjectsWithRecentExpenses();
  }, [getIsMounted]);

  return (
    <>
      <h5>Quick Actions</h5>
      <ul className="list-group quick-actions">
        <li className="list-group-item">
          <Link to="/expense/entry">Enter Expenses for Any Project</Link>
        </li>
        {projects.map((project) => (
          <li key={project.id} className="list-group-item">
            <Link to={`/expense/entry/${project.id}`}>
              Enter Expenses for {project.name}{" "}
              <span className="badge badge-light">Recent</span>
            </Link>
          </li>
        ))}
      </ul>
    </>
  );
};
