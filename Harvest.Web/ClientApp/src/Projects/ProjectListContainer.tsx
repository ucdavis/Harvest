import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Project } from "../types";
import { ProjectTable } from "./ProjectTable";
import { useIsMounted } from "../Shared/UseIsMounted";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus } from "@fortawesome/free-solid-svg-icons";

interface Props {
  projectSource: string;
}

const getListTitle = (projectSource: string) => {
  switch (projectSource) {
    case "/Project/RequiringManagerAttention":
      return "Projects Requiring Manager Attention";
    default:
      return "Projects";
  }
};

export const ProjectListContainer = (props: Props) => {
  const [projects, setProjects] = useState<Project[]>([]);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch(props.projectSource);

      if (response.ok) {
        getIsMounted() && setProjects(await response.json());
      }
    };

    cb();
  }, [props.projectSource, getIsMounted]);

  return (
    <div>
      <div className="row justify-content-between mb-3">
        <div className="col">
          <h1>{getListTitle(props.projectSource)}</h1>
        </div>
        <div className="col text-right">
          <Link to="/request/create" className="btn btn-sm btn-primary ">
            Create New <FontAwesomeIcon icon={faPlus} />
          </Link>
        </div>
      </div>

      <ProjectTable projects={projects}></ProjectTable>
    </div>
  );
};
