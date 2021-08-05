import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Project } from "../types";

export const WorkerHome = () => {
  const [projects, setProjects] = useState<Project[]>([]);

  useEffect(() => {
    const getProjectsWithRecentExpenses = async () => {
      const response = await fetch("/expense/GetRecentExpensedProjects");
      const projects: Project[] = await response.json();
      setProjects(projects);
    };

    getProjectsWithRecentExpenses();
  }, []);

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
