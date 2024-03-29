import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { CommonRouteParams, Project } from "../types";
import { ProjectTable } from "./ProjectTable";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus } from "@fortawesome/free-solid-svg-icons";
import { useParams } from "react-router";

interface Props {
  projectSource: string;
  hasTeamRoute: boolean;
}

const getListTitle = (projectSource: string) => {
  switch (projectSource) {
    case "Project/RequiringManagerAttention":
      return "Projects Requiring Manager Attention";
    case "Project/GetCompleted":
      return "Completed Projects";
    default:
      return "Projects";
  }
};

export const ProjectListContainer = (props: Props) => {
  const [projects, setProjects] = useState<Project[]>([]);

  const { team } = useParams<CommonRouteParams>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get rates so we can load up all expense types and info
    let url = props.projectSource;
    if (props.hasTeamRoute) {
      url = `/api/${team}/${props.projectSource}`;
    }

    const cb = async () => {
      const response = await authenticatedFetch(url);

      if (response.ok) {
        getIsMounted() && setProjects(await response.json());
      }
    };

    cb();
  }, [props.projectSource, props.hasTeamRoute, getIsMounted, team]);

  const requestUrl = !team ? "/request/create" : `/${team}/request/create`;

  return (
    <div className="projectlisttable-wrapper">
      <div className="row justify-content-between mb-3">
        <div className="col">
          <h1>{getListTitle(props.projectSource)}</h1>
        </div>
        <div className="col text-right">
          <Link to={requestUrl} className="btn btn-sm btn-primary ">
            Create New <FontAwesomeIcon icon={faPlus} />
          </Link>
        </div>
      </div>

      {projects.length > 0 ? (
        <ProjectTable projects={projects}></ProjectTable>
      ) : (
        <div className="alert alert-info">No projects found</div>
      )}
    </div>
  );
};
