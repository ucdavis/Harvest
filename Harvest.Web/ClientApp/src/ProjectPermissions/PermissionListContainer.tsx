import React from "react";

import { CommonRouteParams, Project } from "../types";
import { ProjectPermissionTable } from "./ProjectPermissionTable";
import { Link, useParams } from "react-router-dom";
import { ShowForPiOnly } from "../Shared/ShowForPiOnly";

interface Props {
  project: Project;
}

export const PermissionListContainer = (props: Props) => {
  const { project } = props;
  const { team } = useParams<CommonRouteParams>();

  if (project === undefined) {
    return <div>Loading...</div>;
  }

  return (
    <div id="projectPermissionsContainer">
      <div className="row justify-content-between">
        <div className="col">
          <h3>Project Permissions</h3>
        </div>
        <ShowForPiOnly project={project}>
          <div className="col text-right">
            <Link
              to={`/${team}/project/AddProjectPermission/${props.project.id}`}
            >
              Add Permission
            </Link>
          </div>
        </ShowForPiOnly>
      </div>

      <ProjectPermissionTable projectPermissions={project.projectPermissions} />
    </div>
  );
};
