import React, { useContext, useState } from "react";

import { CommonRouteParams, Project, ProjectPermission } from "../types";
import { ProjectPermissionTable } from "./ProjectPermissionTable";
import { Link, useParams } from "react-router-dom";
import { ShowForPiOnly } from "../Shared/ShowForPiOnly";
import { useConfirmationDialog } from "../Shared/ConfirmationDialog";
import AppContext from "../Shared/AppContext";
import { authenticatedFetch } from "../Util/Api";
import { usePromiseNotification } from "../Util/Notifications";

interface Props {
  project: Project;
}

export const PermissionListContainer = (props: Props) => {
  const { project } = props;
  const { team } = useParams<CommonRouteParams>();
  const userInfo = useContext(AppContext);
  const [permissionToDelete, setPermissionToDelete] =
    useState<ProjectPermission | null>(null);
  const [notification, setNotification] = usePromiseNotification();

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

    const request = authenticatedFetch(
      `/api/${team}/Project/RemoveProjectPermission?projectId=${project.id}&permissionId=${permission.id}`,
      {
        method: "POST",
      }
    );

    setNotification(request, "Removing Permission", () => {
      const index = project.projectPermissions.findIndex(
        (p) => p.id === permission.id
      );
      if (index !== -1) {
        project.projectPermissions.splice(index, 1);
      }
      return "Permission removed successfully.";
    });

    // Reset the permission to delete after the operation
    setPermissionToDelete(null);
  };

  return (
    <div id="projectPermissionsContainer">
      <div className="row justify-content-between">
        <div className="col">
          <h3>Project Permissions</h3>
        </div>

        <ShowForPiOnly
          project={project}
          condition={project.originalProjectId === null}
        >
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
          project.originalProjectId === null &&
          (project.principalInvestigator.iam === userInfo.user.detail.iam ||
            project.projectPermissions.some(
              (p) =>
                p.user.iam === userInfo.user.detail.iam &&
                p.permission === "ProjectEditor"
            ))
        }
        deletePermission={deletePermission}
      />
    </div>
  );
};
