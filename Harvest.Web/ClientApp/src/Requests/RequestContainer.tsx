import React, { useEffect, useState } from "react";
import {
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

import { Project } from "../types";

interface RouteParams {
  projectId?: string;
}

export const RequestContainer = () => {
  const [project, setProject] = useState<Project>({ id: 0 } as Project);

  return (
    <Card>
      <CardBody>
        <CardHeader>Create Field Request</CardHeader>
        <Container>
          <Row>
            <Col>
              <FormGroup>
                <Label>When to Start?</Label>
              </FormGroup>
            </Col>
            <Col>
              <FormGroup>
                <Label>When to Finish?</Label>
                TODO
              </FormGroup>
            </Col>
          </Row>
          <Row>
            <Col>
              <FormGroup tag="fieldset">
                <Label>Which type of crop will we grow?</Label>
                TODO
              </FormGroup>
            </Col>
          </Row>
          <Row>
            <Col>
              <FormGroup>
                <Label>Who will be the PI?</Label>
                TODO
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
                  placeholder="Enter a full description of your requirements"
                />
              </FormGroup>
            </Col>
          </Row>
        </Container>
      </CardBody>
    </Card>
  );
};
