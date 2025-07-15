import React, { useContext } from "react";

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
  const [confirmRemovePermission] = useConfirmationDialog({
    title: "Remove Permission",
    message: "Are you sure you want to remove this permission?",
  });
  const userInfo = useContext(AppContext);

  if (project === undefined) {
    return <div>Loading...</div>;
  }

  const deletePermission = async (permission: ProjectPermission) => {
    const confirmed = await confirmRemovePermission();
    if (!confirmed) return;

    // Implement delete logic here
    console.log("Delete permission:", permission);
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
