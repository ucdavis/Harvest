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
      alert("created!");
    } else {
      alert("didn't work");
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
