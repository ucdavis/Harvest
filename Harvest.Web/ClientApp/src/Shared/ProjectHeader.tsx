import React, { useMemo, useState } from "react";
import { Project, ProjectAccount } from "../types";
import {
  Button,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Progress,
} from "reactstrap";
import { convertCamelCase } from "../Util/StringFormatting";
import { ReturnToProject } from "../Shared/ReturnToProject";
import { formatCurrency } from "../Util/NumberFormatting";

interface Props {
  project: Project;
  title: string;
  hideBack?: boolean; // show return to project button
}

export const ProjectHeader = (props: Props) => {
  const { project, title } = props;
  const [modal, setModal] = useState<boolean>(false);
  const toggleModal = () => setModal((modal) => !modal);

  // Function to show a truncated version of project.requirements if the
  // character count is above 256
  const projectReqs = useMemo(() => {
    if (project.requirements.length > 256) {
      return (
        <p>
          {`${project.requirements.substring(0, 256)} ...`}

          <button className="btn btn-link" onClick={toggleModal}>
            See More
          </button>
        </p>
      );
    } else {
      return <p>{project.requirements}</p>;
    }
  }, [project.requirements]);

  const crops = project.crop.split(",").join(", ");

  return (
    <div>
      <div className="card-content project-header">
        <div className="quote-info row">
          <div className="col-md-6">
            <h2 id="request-title">{title}</h2>
            <h3>{project.name}</h3>
            <p className="lede">PI: {project.principalInvestigator.name}</p>
            <p>
              Created {new Date(project.createdOn).toDateString()} by{" "}
              {project.createdBy.name}
            </p>
            {project.accounts.length > 0 && <p className="lede">Accounts</p>}

            {project.accounts.map((acc: ProjectAccount) => (
              <div key={acc.id}>
                <p>
                  {acc.number} {acc.percentage}% <br />{" "}
                  <small> {acc.name} </small>
                </p>
              </div>
            ))}
            <p className="lede">Requirements</p>
            {projectReqs}
            <Modal isOpen={modal} toggle={toggleModal}>
              <ModalHeader toggle={toggleModal}>
                Project Requirements
              </ModalHeader>
              <ModalBody>{project.requirements}</ModalBody>
              <ModalFooter>
                <Button color="danger" onClick={toggleModal}>
                  Close
                </Button>
              </ModalFooter>
            </Modal>
          </div>
          <div className="col-md-6 quote-info-box">
            <div className="row">
              <div className="col-sm-6">
                <p className="lede">Status</p>
                <p className={`project-status-${project.status}`}>
                  {convertCamelCase(project.status)}
                </p>
                <p className="lede">Type</p>
                <p>{project.cropType}</p>
                <p className="lede">Acres</p>
                <p>{project.acres}</p>
              </div>
              <div className="col-sm-6">
                <p className="lede">Timeline</p>
                <p>
                  {new Date(project.start).toLocaleDateString()} through{" "}
                  {new Date(project.end).toLocaleDateString()}
                </p>
                <p className="lede">Crops</p>
                <p>{crops}</p>
              </div>
            </div>
            <div className="row mt-3">
              <div className="progress-bar-details col">
                <p className="lede">Progress</p>
                <div className="row justify-content-between">
                  <div className="col">
                    <p className="mb-1">
                      ${formatCurrency(project.chargedTotal)} Billed
                    </p>
                  </div>
                  <div className="col text-right">
                    <p className="mb-1">
                      $
                      {formatCurrency(
                        project.quoteTotal - project.chargedTotal
                      )}{" "}
                      Remaining
                    </p>
                  </div>
                </div>

                <Progress
                  style={{ width: "100%", height: "14px" }}
                  value={(project.chargedTotal / project.quoteTotal) * 100}
                />
              </div>
            </div>
          </div>
        </div>
      </div>
      {!props.hideBack && <ReturnToProject projectId={project.id} />}
    </div>
  );
};
