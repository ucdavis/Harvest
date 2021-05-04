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
      <button className="primary-btn btn">
        <a href="/request/create">Create New</a>
      </button>
      <ProjectTable projects={projects}></ProjectTable>
    </div>
  );
};
