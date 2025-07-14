import React from "react";

import { Project } from "../types";
import { ProjectPermissionTable } from "./ProjectPermissionTable";

interface Props {
  project: Project;
}

export const PermissionListContainer = (props: Props) => {
  const { project } = props;

  if (project === undefined) {
    return <div>Loading...</div>;
  }

  if (project.projectPermissions.length === 0) {
    return <div>No project permissions found</div>;
  }

  return (
    <div id="projectPermissionsContainer">
      <div className="row justify-content-between">
        <div className="col">
          <h3>Project Permissions</h3>
        </div>
      </div>

      <ProjectPermissionTable projectPermissions={project.projectPermissions} />
    </div>
  );
};
