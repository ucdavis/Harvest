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
import { ValidationError } from "yup";
import DatePicker from "react-date-picker";

import { SearchPerson } from "./SearchPerson";
import { Crops } from "./Crops";
import { requestSchema } from "../schemas";
import { Project } from "../types";

export const RequestContainer = () => {
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
    <Card>
      <CardBody>
        <CardHeader>Create Field Request</CardHeader>
        <Container>
          <Row>
            <Col>
              <FormGroup>
                <Label>When to Start?</Label>
                <DatePicker
                  format="MM/dd/yyyy"
                  required={true}
                  clearIcon={null}
                  value={project.start}
                  onChange={(date) =>
                    setProject({ ...project, start: date as Date })
                  }
                />
              </FormGroup>
            </Col>
            <Col>
              <FormGroup>
                <Label>When to Finish?</Label>
                <DatePicker
                  format="MM/dd/yyyy"
                  required={true}
                  clearIcon={null}
                  value={project.end}
                  onChange={(date) =>
                    setProject({ ...project, end: date as Date })
                  }
                />
              </FormGroup>
            </Col>
          </Row>
          <Row>
            <Col>
              <FormGroup tag="fieldset">
                <Label>Which type of crop will we grow?</Label>
                <Crops
                  crops={project.crop}
                  setCrops={(c) => setProject({ ...project, crop: c })}
                ></Crops>
              </FormGroup>
            </Col>
          </Row>
          <Row>
            <Col>
              <FormGroup>
                <Label>Who will be the PI?</Label>
                <SearchPerson
                  user={project.principalInvestigator}
                  setUser={(u) =>
                    setProject({ ...project, principalInvestigator: u })
                  }
                ></SearchPerson>
              </FormGroup>
            </Col>
          </Row>
          <Row>
            <Col>
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
            </Col>
          </Row>
          <Row>
            <Col>
              <ul>
                {inputErrors.map((error, i) => {
                  return (
                    <li style={{ color: "red" }} key={`error-${i}`}>
                      {error}
                    </li>
                  );
                })}
              </ul>
              <Button color="primary" onClick={create}>
                Create Field Request
              </Button>
            </Col>
          </Row>
        </Container>
        <div>DEBUG: {JSON.stringify(project)}</div>
      </CardBody>
    </Card>
  );
};
