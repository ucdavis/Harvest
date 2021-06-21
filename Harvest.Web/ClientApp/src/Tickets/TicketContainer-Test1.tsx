import React, { useEffect, useState } from "react";
import { useParams } from "react-router";
import { Button, FormGroup, Input, Label } from "reactstrap";
import { ValidationError } from "yup";
import DatePicker from "react-date-picker";



import { Project } from "../types";

interface RouteParams {
  projectId?: string;
}

export const TicketContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>({
    id: 0,
  } as Project);



  useEffect(() => {
    // load original request if this is a change request
    const cb = async () => {
      const response = await fetch(`/Ticket/Get/${projectId}`);

      if (response.ok) {
          const proj: Project = await response.json();
          setProject({
              ...proj
          });
      }
    };

    cb();
  }, [projectId]);

  const create = async () => {
    // TODO: validation, loading spinner
    // create a new project


    const response = await fetch(`/Ticket/Create`, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(project),
    });

    if (response.ok) {
      const data = await response.json();
      window.location.pathname = `/Project/Details/${data.id}`;
    } else {
      alert("Something went wrong, please try again");
    }
  };


  if (projectId === undefined || project.id === 0) {
      // if we have a project id but it hasn't loaded yet, wait
      return <div>ERROR!!! Need to be associated with a project</div>;
  }

  return (
    <div className="card-wrapper card-medium">
      <div className="card-content">
        <div className="card-head">
          <h2>
            Create New Ticket for your project
          </h2>
        </div>
        <div className="row">
          <div className="col-md-6">
            <div className="form-group">
              <Label>Due Date?</Label>
              <div className="input-group" style={{ zIndex: 9000 }}>
                <DatePicker
                  format="MM/dd/yyyy"
                  required={true}
                  clearIcon={null}
                  value={project.start}
                  onChange={(date) =>
                    setProject({ ...project, start: date as Date })
                  }
                />
              </div>
            </div>
          </div>
        </div>



        <FormGroup>
          <Label>What are the details of your ticket request?</Label>
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
        <div className="row justify-content-center">

          <Button className="btn-lg" color="primary" onClick={create}>
            {projectId ? "Create Change Request" : "Create Field Request"}
          </Button>
        </div>
        <div>DEBUG: {JSON.stringify(project)}</div>
      </div>
    </div>
  );
};
