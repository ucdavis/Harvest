import React, { useState } from "react";
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  Col,
  Container,
  FormGroup,
  Input,
  Label,
  Row,
} from "reactstrap";
import DatePicker from "react-date-picker";

import { SearchPerson } from "./SearchPerson";
import { Crops } from "./Crops";
import { Project } from "../types";

export const RequestContainer = () => {
  const [project, setProject] = useState<Project>({ id: 0 } as Project);

  const create = async () => {
    // TODO: validation, loading spinner
    // create a new project
    const response = await fetch(`/Request/Create`, {
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
              <div className="input-group">
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
              <div className="input-group">
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
            />
            <label className="custom-control-label">Row Crops</label>
          </div>
          <div className="custom-control custom-radio">
            <input
              type="radio"
              id="customRadio2"
              name="customRadio"
              className="custom-control-input"
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
          <Button color="primary" onClick={create}>
            Create Field Request
          </Button>
        </div>
        <div>DEBUG: {JSON.stringify(project)}</div>
      </div>
    </div>
  );
};
