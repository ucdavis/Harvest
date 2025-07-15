import React, { useContext, useState } from "react";

import { CommonRouteParams, Project, ProjectPermission } from "../types";
import { ProjectPermissionTable } from "./ProjectPermissionTable";
import { Link, useParams } from "react-router-dom";
import { ShowForPiOnly } from "../Shared/ShowForPiOnly";
import { useConfirmationDialog } from "../Shared/ConfirmationDialog";
import AppContext from "../Shared/AppContext";

interface Props {
  project: Project;
}

export const PermissionListContainer = (props: Props) => {
  const { project } = props;
  const { team } = useParams<CommonRouteParams>();
  const userInfo = useContext(AppContext);
  const [permissionToDelete, setPermissionToDelete] =
    useState<ProjectPermission | null>(null);

  const [confirmRemovePermission] = useConfirmationDialog(
    {
      title: "Remove Permission",
      message: permissionToDelete
        ? `Are you sure you want to remove permission for ${permissionToDelete.user.name}?`
        : "Are you sure you want to remove this permission?",
    },
    [permissionToDelete]
  );

  if (project === undefined) {
    return <div>Loading...</div>;
  }

  const deletePermission = async (permission: ProjectPermission) => {
    setPermissionToDelete(permission);
    const confirmed = await confirmRemovePermission();
    if (!confirmed) {
      setPermissionToDelete(null);
      return;
    }

    // Implement delete logic here
    console.log("Delete permission:", permission);
    setPermissionToDelete(null);
  };

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

      <ProjectPermissionTable
        projectPermissions={project.projectPermissions}
        canDeletePermission={
          project.principalInvestigator.iam === userInfo.user.detail.iam ||
          project.projectPermissions.some(
            (p) =>
              p.user.id === userInfo.user.detail.id &&
              p.permission === "ProjectEditor"
          )
        }
        deletePermission={deletePermission}
      />
    </div>
  );
};
