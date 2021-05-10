import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Project } from "../types";
import { ProjectTable } from "./ProjectTable";

export const ProjectListContainer = () => {
  const [projects, setProjects] = useState<Project[]>([]);

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch("/Project/Active");

      if (response.ok) {
        setProjects(await response.json());
      }
    };

    cb();
  }, []);

  return (
    <div>
      <div className="row justify-content-between mb-3">
        <div className="col">
          <h1>Projects</h1>
        </div>
        <div className="col text-right">
          <Link to="/request/create" className="btn btn-sm btn-link ">
            fontaweomse PLus Icon here Create New
          </Link>
        </div>
      </div>

      <ProjectTable projects={projects}></ProjectTable>
    </div>
  );
};
