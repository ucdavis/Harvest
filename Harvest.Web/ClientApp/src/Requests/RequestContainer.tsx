import React, { useContext, useEffect, useMemo, useState } from "react";
import { useHistory, useParams } from "react-router";
import { Button, FormGroup, Input, Label } from "reactstrap";
import { ValidationError } from "yup";
import DatePicker from "react-date-picker";

import { FileUpload } from "../Shared/FileUpload";
import { SearchPerson } from "./SearchPerson";
import { Crops } from "./Crops";
import { requestSchema } from "../schemas";
import { Project, CropType } from "../types";
import AppContext from "../Shared/AppContext";
import { usePromiseNotification } from "../Util/Notifications";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { useIsMounted } from "../Shared/UseIsMounted";

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
  const [inputErrors, setInputErrors] = useState<string[]>([]);

  const [notification, setNotification] = usePromiseNotification();

  const checkRequestValidity = async (inputs: any) => {
    try {
      await requestSchema.validate(inputs, { abortEarly: false });
    } catch (err) {
      if (err instanceof ValidationError) {
        return err.errors;
      }
    }
  };

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
    const requestErrors = await checkRequestValidity(project);

    if (requestErrors) {
      if (requestErrors.length > 0) {
        setInputErrors(requestErrors);

        if (new Date(project.start) > new Date(project.end)) {
          setInputErrors((oldErrors) => [
            ...oldErrors,
            "Start date must be before end date",
          ]);
        }
        return;
      } else {
        setInputErrors([]);
      }
    }

    if (new Date(project.start) > new Date(project.end)) {
      setInputErrors(["Start date must be before end date"]);
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
    <div className="card-wrapper">
      {originalProject !== undefined && (
        <ProjectHeader
          project={originalProject}
          title={"Original Field Request #" + (originalProject?.id || "")}
        />
      )}
      <div className="card-green-bg">
        <div className="row justify-content-center">
          <div className="col-md-6 card-wrapper no-green mt-4 mb-4">
            <div className="card-content">
              <h2>
                {projectId ? "Create Change Request" : "Create Field Request"}
              </h2>
              <div className="row">
                <div className="col-md-6">
                  <div className="form-group">
                    <Label>When to Start?</Label>
                    <div className="input-group" style={{ zIndex: 9000 }}>
                      <DatePicker
                        format="MM/dd/yyyy"
                        required={false}
                        clearIcon={null}
                        value={project.start}
                        onChange={(date: Date) =>
                          setProject({ ...project, start: date })
                        }
                      />
                    </div>
                  </div>
                </div>
                <div className="col-md-6">
                  <FormGroup>
                    <Label>When to Finish?</Label>
                    <div className="input-group" style={{ zIndex: 9000 }}>
                      <DatePicker
                        format="MM/dd/yyyy"
                        required={false}
                        clearIcon={null}
                        value={project.end}
                        onChange={(date: Date) =>
                          setProject({ ...project, end: date })
                        }
                      />
                    </div>
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
                  setCrops={(c) => setProject({ ...project, crop: c })}
                  cropType={project.cropType}
                />
              </FormGroup>

              <FormGroup>
                <Label>Who will be the PI?</Label>
                <SearchPerson
                  user={project.principalInvestigator}
                  setUser={(u) =>
                    setProject({ ...project, principalInvestigator: u })
                  }
                />
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

                      return { ...proj, attachments: [...proj.attachments] };
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
                  onChange={(e) =>
                    setProject({ ...project, requirements: e.target.value })
                  }
                  placeholder="Enter a full description of your requirements"
                />
              </FormGroup>
              <ul>
                {inputErrors.map((error, i) => {
                  return (
                    <li style={{ color: "red" }} key={`error-${i}`}>
                      {error}
                    </li>
                  );
                })}
              </ul>
              <div className="row justify-content-center">
                <Button
                  className="btn-lg"
                  color="primary"
                  onClick={create}
                  disabled={!isFilledIn || notification.pending}
                >
                  {projectId ? "Create Change Request" : "Create Field Request"}
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
