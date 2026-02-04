import React, { useEffect, useState } from "react";
import { useHistory, useParams } from "react-router-dom";
import { Button, FormGroup, Input, Label } from "reactstrap";
import DatePicker from "react-datepicker";

import { CommonRouteParams, Project } from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { usePromiseNotification } from "../Util/Notifications";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";

interface RouteParams {
  projectId?: string;
}

export const OverrideProjectContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const { team } = useParams<CommonRouteParams>();
  const [project, setProject] = useState<Project | undefined>();
  const [projectName, setProjectName] = useState("");
  const [startDate, setStartDate] = useState<Date | null>(null);
  const [endDate, setEndDate] = useState<Date | null>(null);
  const [notification, setNotification] = usePromiseNotification();
  const history = useHistory();

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
          setProjectName(proj.name || "");
          setStartDate(new Date(proj.start));
          setEndDate(new Date(proj.end));
        }
      }
    };

    if (projectId) {
      cb();
    }
  }, [projectId, getIsMounted, team]);

  const handleSave = async () => {
    if (!projectName || !startDate || !endDate || !project) {
      return;
    }

    const updatedProject = {
      ...project,
      name: projectName,
      start: startDate,
      end: endDate,
    };

    const request = authenticatedFetch(
      `/api/${team}/Project/OverrideProject/${projectId}`,
      {
        method: "POST",
        body: JSON.stringify(updatedProject),
      }
    );

    setNotification(request, "Saving Project Override", "Project Updated");

    const response = await request;

    if (response.ok) {
      history.push(`/${team}/project/details/${projectId}`);
    }
  };

  if (project === undefined) {
    return <div>Loading...</div>;
  }

  const isFormValid =
    !!projectName && !!startDate && !!endDate && startDate <= endDate;

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Override Field Request #" + (project.id || "")}
      />
      <div className="card-green-bg">
        <div className="card-content">
          <div className="row">
            <div className="col-md-8 offset-md-2">
              <h3>Override Project Details</h3>
              <p className="text-muted">
                This allows field managers to override project name and dates
                for requested projects.
              </p>
              <p className="text-muted">
                IMPORTANT! If the number of years changes, the quote should be
                updated accordingly.
              </p>

              <FormGroup>
                <Label for="projectName">Project Name</Label>
                <Input
                  type="text"
                  id="projectName"
                  value={projectName}
                  onChange={(e) => setProjectName(e.target.value)}
                  placeholder="Enter project name"
                />
              </FormGroup>

              <div className="row">
                <div className="col-md-6">
                  <FormGroup>
                    <Label>Start Date</Label>
                    <div className="input-group" style={{ zIndex: 9000 }}>
                      <DatePicker
                        className="form-control"
                        selected={startDate}
                        onChange={(date: Date | null) => setStartDate(date)}
                        isClearable
                      />
                    </div>
                  </FormGroup>
                </div>
                <div className="col-md-6">
                  <FormGroup>
                    <Label>End Date</Label>
                    <div className="input-group" style={{ zIndex: 9000 }}>
                      <DatePicker
                        className="form-control"
                        selected={endDate}
                        onChange={(date: Date | null) => setEndDate(date)}
                        isClearable
                      />
                    </div>
                  </FormGroup>
                </div>
              </div>

              <div className="mt-4">
                <Button
                  color="primary"
                  disabled={!isFormValid || notification.pending}
                  onClick={handleSave}
                >
                  Save Changes
                </Button>{" "}
                <Button
                  color="secondary"
                  className="btn btn-danger"
                  onClick={() =>
                    history.push(`/${team}/project/details/${projectId}`)
                  }
                  disabled={notification.pending}
                >
                  Cancel
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
