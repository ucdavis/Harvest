import React, { useMemo, useState, useEffect } from "react";
import { CommonRouteParams, Project, ProjectAccount } from "../types";
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
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faEye } from "@fortawesome/free-solid-svg-icons";
import { ShowForPiOnly } from "./ShowForPiOnly";
import { useParams } from "react-router-dom";
import { ShowFor } from "./ShowFor";
import { authenticatedFetch } from "../Util/Api";
import { useIsMounted } from "../Shared/UseIsMounted";

interface Props {
  project: Project;
  title: string;
  hideBack?: boolean; // show return to project button
}

export const ProjectHeader = (props: Props) => {
  const { project, title } = props;
  const [modal, setModal] = useState<boolean>(false);
  const [originalProject, setOriginalProject] = useState<Project | undefined>(
    undefined
  );
  const [isLoadingOriginalProject, setIsLoadingOriginalProject] =
    useState<boolean>(false);
  const toggleModal = () => setModal((modal) => !modal);
  const { shareId } = useParams<CommonRouteParams>();
  const getIsMounted = useIsMounted();

  // Fetch original project when status is ChangeRequested and originalProjectId exists
  useEffect(() => {
    const fetchOriginalProject = async () => {
      if (project.status === "ChangeRequested" && project.originalProjectId) {
        setIsLoadingOriginalProject(true);
        try {
          const response = await authenticatedFetch(
            `/api/${project.team.slug}/Project/Get/${project.originalProjectId}`
          );
          if (response.ok && getIsMounted()) {
            const originalProj: Project = await response.json();
            setOriginalProject(originalProj);
          }
        } catch (error) {
          console.error("Error fetching original project:", error);
        } finally {
          if (getIsMounted()) {
            setIsLoadingOriginalProject(false);
          }
        }
      }
    };

    fetchOriginalProject();
  }, [
    project.status,
    project.originalProjectId,
    project.team.slug,
    getIsMounted,
  ]);

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
            <p className="lede">
              PI: {project.principalInvestigator.name}{" "}
              <a
                href={`https://who.ucdavis.edu/Detail/${project.principalInvestigator.kerberos}`}
                target="_blank"
                rel="noreferrer"
              >
                <FontAwesomeIcon icon={faEye} />
              </a>
            </p>
            <p>
              Created {new Date(project.createdOn).toDateString()} by{" "}
              {project.createdBy.name}{" "}
              <a
                href={`https://who.ucdavis.edu/Detail/${project.createdBy.kerberos}`}
                target="_blank"
                rel="noreferrer"
              >
                <FontAwesomeIcon icon={faEye} />
              </a>
            </p>
            <ShowForPiOnly project={project}>
              <p className="lede">Share Link:</p>
              <p>
                <a
                  href={`${window.location.origin}/${project.team.slug}/project/details/${project.id}/${project.shareId}`}
                >
                  {`${window.location.origin}/${project.team.slug}/project/details/${project.id}/${project.shareId}`}
                </a>
              </p>
            </ShowForPiOnly>
            {project.accounts.length > 0 && <p className="lede">Accounts</p>}

            {project.accounts.map((acc: ProjectAccount) => (
              <div key={acc.id}>
                <p className="word-wrap">
                  <a
                    href={`https://finjector.ucdavis.edu/Details/${acc.number}`}
                    target="_blank"
                    rel="noreferrer"
                  >
                    {acc.number}{" "}
                  </a>
                  | <b>{acc.percentage}%</b> <br /> <small> {acc.name} </small>
                </p>
              </div>
            ))}
            <p className="lede">Requirements</p>
            {projectReqs}
            <Modal isOpen={modal} toggle={toggleModal}>
              <ModalHeader
                toggle={toggleModal}
                close={
                  <button className="close" onClick={toggleModal}>
                    &times;
                  </button>
                }
              >
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
          <div className="col-md-6">
            <div className="quote-info-box">
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
                  <ShowFor condition={project.status === "ChangeRequested"}>
                    {isLoadingOriginalProject ? (
                      <p className="text-muted">
                        <small>Loading original project dates...</small>
                      </p>
                    ) : originalProject ? (
                      <>
                        {(new Date(project.start).toLocaleDateString() !==
                          new Date(
                            originalProject.start
                          ).toLocaleDateString() ||
                          new Date(project.end).toLocaleDateString() !==
                            new Date(
                              originalProject.end
                            ).toLocaleDateString()) && (
                          <div className="text-danger">
                            <small>
                              <b>Original dates:</b>{" "}
                              {new Date(
                                originalProject.start
                              ).toLocaleDateString()}{" "}
                              through{" "}
                              {new Date(
                                originalProject.end
                              ).toLocaleDateString()}
                            </small>
                          </div>
                        )}
                      </>
                    ) : null}
                  </ShowFor>
                  <p className="lede">Team</p>
                  <p>{project.team.name}</p>
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
      </div>
      {!props.hideBack && (
        <ReturnToProject
          projectId={project.id}
          team={project.team.slug}
          shareId={shareId}
        />
      )}
    </div>
  );
};
