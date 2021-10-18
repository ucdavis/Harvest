import React, { useContext, useEffect, useMemo, useState } from "react";
import { useHistory, useParams } from "react-router";
import { Link } from "react-router-dom";
import { Button, FormGroup, Input, Label } from "reactstrap";
import DatePicker from "react-datepicker";

import { FileUpload } from "../Shared/FileUpload";
import { SearchPerson } from "./SearchPerson";
import { Crops } from "./Crops";
import { requestSchema } from "../schemas";
import { Project, CropType } from "../types";
import AppContext from "../Shared/AppContext";
import { usePromiseNotification } from "../Util/Notifications";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { useIsMounted } from "../Shared/UseIsMounted";
import { useInputValidator, ValidationProvider } from "use-input-validator";
import { validatorOptions } from "../constants";

interface RouteParams {
  projectId?: string;
}

export const RequestContainer = () => {
  const history = useHistory();
  const { detail: userDetail, roles: userRoles } = useContext(AppContext).user;

  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>({
    id: 0,
    cropType: "Row" as CropType,
    principalInvestigator: userDetail,
  } as Project);
  const [originalProject, setOriginalProject] = useState<Project>();

  const {
    context,
    validateAll,
    formErrorCount,
    formIsDirty,
    onChange,
    onChangeValue,
    onBlur,
    onBlurValue,
    InputErrorMessage,
    propertyHasErrors,
  } = useInputValidator(requestSchema, project, validatorOptions);

  const [notification, setNotification] = usePromiseNotification();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // load original request if this is a change request
    const cb = async () => {
      const response = await fetch(`/Project/Get/${projectId}`);

      if (response.ok) {
        const proj: Project = await response.json();

        if (getIsMounted()) {
          setProject({
            ...proj,
            start: new Date(proj.start),
            end: new Date(proj.end),
            requirements: `Original: ${proj.requirements}`,
          });
          setOriginalProject(proj);
        }
      }
    };

    if (projectId !== undefined) {
      cb();
    }
  }, [projectId, getIsMounted]);

  const create = async () => {
    // TODO: validation, loading spinner
    // create a new project
    const requestErrors = await validateAll();

    if (requestErrors.length > 0) {
      return;
    }

    const request = fetch(`/Request/Create`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(project),
    });

    if (project.principalInvestigator.iam !== userDetail.iam) {
      setNotification(
        request,
        `Creating Request For ${project.principalInvestigator.name}`,
        `Request Created For ${project.principalInvestigator.name}`
      );
    } else {
      setNotification(request, "Creating Request", "Request Created");
    }

    const response = await request;

    if (response.ok) {
      // if user is becoming a PI for first time, grant them the PI role
      if (
        project.principalInvestigator.id === userDetail.id &&
        !userRoles.includes("PI")
      ) {
        userRoles.push("PI");
      }
      const data = await response.json();
      if (data.principalInvestigator.id !== data.createdBy.id) {
        history.push("/");
      } else {
        history.push(`/Project/Details/${data.id}`);
      }
    }
  };

  const handleCropTypeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setProject({ ...project, cropType: e.target.value as CropType });
  };

  var isFilledIn = useMemo(() => {
    return project.start && project.end && project.crop && project.requirements;
  }, [project.start, project.end, project.crop, project.requirements]);

  if (projectId !== undefined && project.id === 0) {
    // if we have a project id but it hasn't loaded yet, wait
    return <div>Loading...</div>;
  }

  return (
    <div>
      {originalProject !== undefined && (
        <div className="alert alert-info">
          You are making a change request for {project.name}{" "}
          <Link className="alert-link" to={`/project/details/${project.id}`}>
            Click here to go back to the project details page
          </Link>
        </div>
      )}
      <div className={originalProject && "card-wrapper"}>
        <ValidationProvider context={context}>
          {originalProject !== undefined && (
            <ProjectHeader
              project={originalProject}
              title={"Original Field Request #" + (originalProject?.id || "")}
            />
          )}
          <div className={originalProject && "card-green-bg"}>
            <div className="row justify-content-center">
              <div className="col-md-6 card-wrapper no-green mt-4 mb-4">
                <div className="card-content">
                  <h2>
                    {projectId
                      ? "Create Change Request"
                      : "Create Field Request"}
                  </h2>
                  <div className="row">
                    <div className="col-md-6">
                      <div className="form-group">
                        <Label>When to Start?</Label>
                        <div
                          className="input-group"
                          style={{ zIndex: 9000 }}
                          onBlur={() => onBlurValue("start")}
                        >
                          <DatePicker
                            className="form-control"
                            selected={project.start}
                            onChange={onChangeValue("start", (date: Date) =>
                              setProject({ ...project, start: date })
                            )}
                            isClearable
                          />
                        </div>
                        <InputErrorMessage name="start" />
                      </div>
                    </div>
                    <div className="col-md-6">
                      <FormGroup>
                        <Label>When to Finish?</Label>
                        <div
                          className="input-group"
                          style={{ zIndex: 9000 }}
                          onBlur={() => onBlurValue("end")}
                        >
                          <DatePicker
                            className="form-control"
                            selected={project.end}
                            onChange={onChangeValue("end", (date: Date) =>
                              setProject({ ...project, end: date })
                            )}
                            isClearable
                          />
                        </div>
                        <InputErrorMessage name="end" />
                      </FormGroup>
                    </div>
                  </div>
                  <FormGroup>
                    <Label>Which type of crop will we grow?</Label>
                    <div className="custom-control custom-radio">
                      <input
                        type="radio"
                        id="rowCropInput"
                        name="rowCropInput"
                        className="custom-control-input"
                        style={{ zIndex: 1 }} //prevent class custom-control-input from blocking mouse clicks
                        value="Row"
                        checked={project.cropType === "Row"}
                        onChange={handleCropTypeChange}
                      />
                      <label
                        className="custom-control-label"
                        htmlFor="rowCropInput"
                      >
                        Row Crops
                      </label>
                    </div>
                    <div className="custom-control custom-radio">
                      <input
                        type="radio"
                        id="treeCropInput"
                        name="treeCropInput"
                        className="custom-control-input"
                        style={{ zIndex: 1 }} //prevent class custom-control-input from blocking mouse clicks
                        value="Tree"
                        checked={project.cropType === "Tree"}
                        onChange={handleCropTypeChange}
                      />
                      <label
                        className="custom-control-label"
                        htmlFor="treeCropInput"
                      >
                        Tree Crops
                      </label>
                    </div>
                  </FormGroup>

                  <FormGroup tag="fieldset">
                    <Label>What crop(s) will we grow?</Label>
                    <Crops
                      crops={project.crop}
                      onChange={onChangeValue("crop", (c) =>
                        setProject({ ...project, crop: c })
                      )}
                      cropType={project.cropType}
                      onBlur={() => onBlurValue("crop")}
                    />
                    <InputErrorMessage name="crop" />
                  </FormGroup>

                  <FormGroup>
                    <Label>Who will be the PI?</Label>
                    <SearchPerson
                      user={project.principalInvestigator}
                      onChange={onChangeValue("principalInvestigator", (u) =>
                        setProject({ ...project, principalInvestigator: u })
                      )}
                      onBlur={() => onBlurValue("principalInvestigator")}
                    />
                    <InputErrorMessage name="principalInvestigator" />
                    {!propertyHasErrors("principalInvestigator") && (
                      <small id="PIHelp" className="form-text text-muted">
                        Enter PI Email or Kerberos. Click [x] to clear out an
                        existing PI.
                      </small>
                    )}
                  </FormGroup>

                  <FormGroup>
                    <Label>Want to attach any files?</Label>
                    <FileUpload
                      files={project.attachments || []}
                      setFiles={(f) =>
                        setProject({ ...project, attachments: [...f] })
                      }
                      updateFile={(f) =>
                        setProject((proj) => {
                          // update just one specific file from project p
                          proj.attachments[
                            proj.attachments.findIndex(
                              (file) => file.identifier === f.identifier
                            )
                          ] = { ...f };

                          return {
                            ...proj,
                            attachments: [...proj.attachments],
                          };
                        })
                      }
                    />
                  </FormGroup>

                  <FormGroup>
                    <Label>What are the requirements?</Label>
                    <Input
                      type="textarea"
                      name="text"
                      id="requirements"
                      value={project.requirements}
                      onChange={onChange("requirements", (e) =>
                        setProject({ ...project, requirements: e.target.value })
                      )}
                      onBlur={onBlur("requirements")}
                      placeholder="Enter a full description of your requirements"
                    />
                    <InputErrorMessage name="requirements" />
                  </FormGroup>
                  <div className="row justify-content-center">
                    <Button
                      className="btn-lg"
                      color="primary"
                      onClick={create}
                      disabled={
                        !isFilledIn ||
                        notification.pending ||
                        formErrorCount > 0 ||
                        !formIsDirty
                      }
                    >
                      {projectId
                        ? "Create Change Request"
                        : "Create Field Request"}
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </ValidationProvider>
      </div>
    </div>
  );
};
