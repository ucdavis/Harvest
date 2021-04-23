import React, { useState } from "react";
import { useParams } from "react-router";
import { Button, FormGroup, Input, Label } from "reactstrap";
import { ValidationError } from "yup";
import DatePicker from "react-date-picker";

import { SearchPerson } from "./SearchPerson";
import { Crops } from "./Crops";
import { requestSchema } from "../schemas";
import { Project, CropType } from "../types";

interface RouteParams {
  projectId?: string;
}

export const RequestContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>({ id: 0 } as Project);
  const [inputErrors, setInputErrors] = useState<string[]>([]);

  const checkRequestValidity = async (inputs: any) => {
    try {
      await requestSchema.validate(inputs, { abortEarly: false });
    } catch (err) {
      if (err instanceof ValidationError) {
        return err.errors;
      }
    }
  };

  const create = async () => {
    // TODO: validation, loading spinner
    // create a new project
    const requestErrors = await checkRequestValidity(project);

    if (requestErrors) {
      if (requestErrors.length > 0) {
        setInputErrors(requestErrors);
        return;
      }
    }

    // const response = await fetch(`/Request/Create`, {
    //   method: "POST",
    //   headers: {
    //     Accept: "application/json",
    //     "Content-Type": "application/json",
    //   },
    //   body: JSON.stringify(project),
    // });

    // if (response.ok) {
    //   const data = await response.json();
    //   window.location.pathname = `/Project/Details/${data.id}`;
    // } else {
    //   alert("Something went wrong, please try again");
    // }
  };

  const handleCropTypeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setProject({ ...project, cropType: e.target.value as CropType });
  };

  if (projectId !== undefined && project.id === 0) {
    // if we have a project id but it hasn't loaded yet, wait
    return <div>Loading...</div>;
  }

  return (
    <div className="card-wrapper card-medium">
      <div className="card-content">
        <div className="card-head">
          <h2>Create Field Request</h2>
        </div>
        <div className="row">
          <div className="col-md-6">
            <div className="form-group">
              <Label>When to Start?</Label>
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
          <div className="col-md-6">
            <FormGroup>
              <Label>When to Finish?</Label>
              <div className="input-group" style={{ zIndex: 9000 }}>
                <DatePicker
                  format="MM/dd/yyyy"
                  required={true}
                  clearIcon={null}
                  value={project.end}
                  onChange={(date) =>
                    setProject({ ...project, end: date as Date })
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
              id="customRadio1"
              name="customRadio"
              className="custom-control-input"
              style={{ zIndex: 1 }} //prevent class custom-control-input from blocking mouse clicks
              value="Row"
              checked={project.cropType === "Row"}
              onChange={handleCropTypeChange}
            />
            <label className="custom-control-label">Row Crops</label>
          </div>
          <div className="custom-control custom-radio">
            <input
              type="radio"
              id="customRadio2"
              name="customRadio"
              className="custom-control-input"
              style={{ zIndex: 1 }} //prevent class custom-control-input from blocking mouse clicks
              value="Tree"
              checked={project.cropType === "Tree"}
              onChange={handleCropTypeChange}
            />
            <label className="custom-control-label">Tree Crops</label>
          </div>
        </FormGroup>

        <FormGroup tag="fieldset">
          <Label>What crop(s) will we grow?</Label>
          <Crops
            crops={project.crop}
            setCrops={(c) => setProject({ ...project, crop: c })}
          ></Crops>
        </FormGroup>

        <FormGroup>
          <Label>Who will be the PI?</Label>
          <SearchPerson
            user={project.principalInvestigator}
            setUser={(u) =>
              setProject({ ...project, principalInvestigator: u })
            }
          ></SearchPerson>
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
        <div className="row justify-content-center">
          <ul>
            {inputErrors.map((error, i) => {
              return (
                <li style={{ color: "red" }} key={`error-${i}`}>
                  {error}
                </li>
              );
            })}
          </ul>
          <Button className="btn-lg" color="primary" onClick={create}>
            Create Field Request
          </Button>
        </div>
        <div>DEBUG: {JSON.stringify(project)}</div>
      </div>
    </div>
  );
};
