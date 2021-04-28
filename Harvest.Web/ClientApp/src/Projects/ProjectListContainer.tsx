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
      <Link to="/request/create">Create New</Link>
      <ProjectTable projects={projects}></ProjectTable>
    </div>
  );
};
