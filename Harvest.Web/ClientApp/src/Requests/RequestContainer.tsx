import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import DatePicker from "reactstrap-date-picker";
import {
  Card, CardBody, CardHeader, CardText, Container, Row, Col, FormGroup, Label, FormText, Input,
  Dropdown, DropdownMenu, DropdownItem, DropdownToggle, Badge, Button
} from "reactstrap";

import { Project, Request, CropType, Crop, User } from "../types";
import { parseIsoDate } from "../utilities/dates";
import { SearchSelect } from "../components/SearchSelect";

interface RouteParams {
  projectId?: string;
}

const defaultRequest = {
  id: 0,
  projectId: 0,
  project: null,
  requirements: "",
  start: new Date(),
  end: new Date(),
  initatedById: 0,
  initiatedBy: null,
  cropType: CropType.Row,
  crops: "",
  principalInvestigatorId: 0,
  principalInvestigator: null,

  approvedById: null,
  approvedBy: null,
  approvedOn: null,

  createdDate: new Date(),
  status: "",
} as Request;

export const RequestContainer = () => {
  //const { projectId } = useParams<RouteParams>({projectId: ''});
  const [project, setProject] = useState<Project>();

  const [request, setRequest] = useState<Request>({ start: new Date(), cropType: CropType.Row } as Request);
  const [cropSuggestionsOpen, setCropSuggestionsOpen] = useState(false);

  const handleSearchCrops = async (query: string) => {
    return [{ id: 1, name: "tomato" }, { id: 2, name: "cucumber" }] as Crop[];
  };

  const handleSearchUser = async (query: string) => {
    return [{ id: 1, firstName: "John", lastName: "Doe", email: "jdoe@email.com", iam: "1234-1234-134-1234", kerberos: "jdoe", name: "John Doe" }] as User[];
  };

  return (
    <Card>
      <CardBody>
        <CardHeader id="request-title">Create Field Request</CardHeader>
        <Container>
          <Row>
            <Col>
              <FormGroup>
                <Label>When to Start?</Label>
                <DatePicker id="start-datepicker"
                  value={request.start.toISOString()}
                  onChange={(v: string, f: string) => setRequest({ ...request, start: parseIsoDate(v) } as Request)} />
              </FormGroup>
            </Col>
            <Col>
              <FormGroup>
                <Label>When to Finish?</Label>
                <DatePicker id="finish-datepicker"
                  value={request.start.toISOString()}
                  onChange={(v: string, f: string) => setRequest({ ...request, end: parseIsoDate(v) } as Request)} />
              </FormGroup>
            </Col>
          </Row>
          <Row>
            <Col>
              <FormGroup tag="fieldset">
                <Label>Which type of crop will we grow?</Label>
                {Object.keys(CropType).map(key => (
                  <FormGroup check key={`cropType_${key}`}>
                    <Label check>
                      <Input type="radio" name="cropType" checked={request.cropType === key}
                        onChange={(e: React.ChangeEvent<HTMLInputElement>) => setRequest({ ...request, cropType: key as CropType })} />{" "}
                      {CropType[key as CropType]}
                    </Label>
                  </FormGroup>
                ))}
              </FormGroup>
            </Col>
          </Row>
          <Row>
            <Col>
              <FormGroup>
                <Label>What crop(s) will we grow?</Label>
                <SearchSelect
                  getText={(item: Crop) => item.name}
                  getId={(item:Crop) => item.id}
                  onCreate={async (name) => { return { id: 1, name: name} as Crop }}
                  onSelectionChanged={(selection) => { console.debug(JSON.stringify(selection)); }}
                  onSearch={handleSearchCrops}
                  selection={[]}
                  placeholder="Select crop(s)"
                  multiselect={true}
                />
              </FormGroup>
            </Col>
          </Row>
          <Row>
            <Col>
              <FormGroup>
                <Label>Who will be the PI?</Label>
                <SearchSelect
                  getText={(user: User) => `${user.lastName}, ${user.firstName} (${user.email})`}
                  getId={(user: User) => user.id}
                  onSelectionChanged={(selection) => { console.debug(JSON.stringify(selection)); }}
                  onSearch={handleSearchUser}
                  selection={[]}
                  placeholder="Enter kerberos or email address"
                />
              </FormGroup>
            </Col>
          </Row>
          <Row>
            <Col>
              <FormGroup>
                <Label>Who should be notified?</Label>
                <Input type="email" name="email" id="exampleEmail" placeholder="Enter email address" />
              </FormGroup>
            </Col>
          </Row>
          <Row>
            <Col>
              <FormGroup>
                <Label>What are the requirements?</Label>
                <Input type="textarea" name="text" id="exampleText" />
              </FormGroup>
            </Col>
          </Row>
        </Container>

        <CardText>

        </CardText>
      </CardBody>
      {/*<div>Debug: {JSON.stringify(quote)}</div>*/}
    </Card>
  );
};
