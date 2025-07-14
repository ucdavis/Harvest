import React, { useEffect, useState } from "react";
import { useHistory, useParams } from "react-router-dom";
import { CommonRouteParams, Project, ProjectPermission } from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { Button, FormGroup, Input, Label } from "reactstrap";
import { projectPermissionSchema } from "../schemas";
import { authenticatedFetch } from "../Util/Api";
import { usePromiseNotification } from "../Util/Notifications";
import { useIsMounted } from "../Shared/UseIsMounted";
import { useInputValidator } from "use-input-validator";
import { validatorOptions } from "../constants";
import { SearchPerson } from "../Requests/SearchPerson";
import { ShowForPiOnly } from "../Shared/ShowForPiOnly";

export const AddProjectPermission = () => {
  const { projectId, team } = useParams<CommonRouteParams>();
  const [project, setProject] = useState<Project>();
  const [projectPermission, setProjectPermissions] =
    useState<ProjectPermission>({
      permission: "",
    } as ProjectPermission);
  const history = useHistory();

  const {
    onChange,
    onChangeValue,
    InputErrorMessage,
    getClassName,
    onBlur,
    onBlurValue,
    formErrorCount,
    formIsTouched,
    validateAll,
    propertyHasErrors,
  } = useInputValidator(
    projectPermissionSchema,
    projectPermission,
    validatorOptions
  );

  const [notification, setNotification] = usePromiseNotification();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/Project/Get/${projectId}`
      );

      if (response.ok) {
        const proj: Project = await response.json();
        if (getIsMounted()) {
          setProject(proj);
          setProjectPermissions((t) => ({ ...t, projectId: proj.id }));
        }
      }
    };

    cb();
  }, [projectId, getIsMounted, team]);

  if (project === undefined) {
    return <div>Loading...</div>;
  }

  const create = async () => {
    const permissionErrors = await validateAll();

    if (permissionErrors.length > 0) {
      return;
    }

    const request = authenticatedFetch(
      `/api/${team}/Project/AddProjectPermission?projectId=${projectId}`,
      {
        method: "POST",
        body: JSON.stringify(projectPermission),
      }
    );
    setNotification(
      request,
      "Adding Project Permission",
      "Project Permission Added"
    );

    const response = await request;

    if (response.ok) {
      const data = await response.json();
      console.log("Project Permission Added: ", data);
      history.push(`/${team}/Project/Details/${project.id}`);
    }
    if (response.status === 400) {
      const data = await response.json();
      console.error("Error adding project permission: ", data);
      setProjectPermissions((prev) => ({
        ...prev,
        permission: "",
      }));
    }
  };

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project.id || "")}
      ></ProjectHeader>
      <div className="card-green-bg pb-3">
        <div className="create-card col-md-8 col-lg-6 card-wrapper no-green mt-4 mb-4">
          <div className="card-content">
            <h2>Lookup User, set Permission</h2>
            <FormGroup>
              <Label>User to Add</Label>
              <SearchPerson
                onChange={onChangeValue("user", (u) =>
                  setProjectPermissions({
                    ...projectPermission,
                    user: u,
                  })
                )}
                onBlur={() => onBlurValue("user")}
              />
              <InputErrorMessage name="user" />
              {!propertyHasErrors("user") && (
                <small id="userHelp" className="form-text text-muted">
                  Enter User Email or Kerberos. Click [x] to clear out an
                  existing User.
                </small>
              )}
            </FormGroup>

            <FormGroup>
              <Label for="permission">Permission</Label>
              <br />
              <Input
                type="select"
                name="permission"
                id="permission"
                value={projectPermission.permission}
                onChange={(e) => {
                  const value = e.target.value;
                  setProjectPermissions({
                    ...projectPermission,
                    permission: value,
                  });
                }}
                onBlur={() => onBlurValue("permission")}
              >
                <option value="">Select Permission</option>
                <option value="ProjectViewer">Project Viewer</option>
                <option value="ProjectEditor">Project Editor</option>
              </Input>
              <InputErrorMessage name="permission" />
              <small id="permissionHelp" className="form-text text-muted">
                Select the permission level for the user.
                <br />
                Project Edit allows the user the same permissions as the PI
              </small>
            </FormGroup>
            <div className="row justify-content-center">
              <ShowForPiOnly project={project}>
                <Button
                  className="btn-lg mb-2"
                  color="primary"
                  onClick={create}
                  disabled={
                    notification.pending || formErrorCount > 0 || !formIsTouched
                  }
                >
                  Add Project Permission
                </Button>
              </ShowForPiOnly>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
