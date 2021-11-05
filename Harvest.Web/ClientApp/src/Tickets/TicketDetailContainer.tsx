import React, { useEffect, useState } from "react";
import { useHistory, useParams } from "react-router-dom";
import { Project, TicketDetails } from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { TicketAttachments } from "./TicketAttachments";
import { TicketMessages } from "./TicketMessages";
import { ShowFor } from "../Shared/ShowFor";
import { TicketWorkNotesEdit } from "./TicketWorkNotesEdit";
import { TicketReply } from "./TicketReply";
import { Button } from "reactstrap";
import { usePromiseNotification } from "../Util/Notifications";
import { useIsMounted } from "../Shared/UseIsMounted";

interface RouteParams {
  projectId: string;
  ticketId: string;
}

export const TicketDetailContainer = () => {
  const { projectId, ticketId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();
  const [ticket, setTicket] = useState<TicketDetails>();
  const history = useHistory();

  const [notification, setNotification] = usePromiseNotification();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const response = await fetch(`/api/Project/Get/${projectId}`);

      if (response.ok) {
        const proj: Project = await response.json();
        getIsMounted() && setProject(proj);
      }
    };

    cb();
  }, [projectId, getIsMounted]);

  useEffect(() => {
    const cb = async () => {
      const response = await fetch(`/api/Ticket/Get/${projectId}/${ticketId}`);

      if (response.ok) {
        const tick: TicketDetails = await response.json();
        getIsMounted() && setTicket(tick);
      }
    };

    cb();
  }, [ticketId, projectId, getIsMounted]);

  if (project === undefined || ticket === undefined) {
    return <div>Loading...</div>;
  }

  const closeTicket = async () => {
    const request = fetch(
      `/api/Ticket/Close?projectId=${projectId}&ticketId=${ticketId}`,
      {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
      }
    );
    setNotification(request, "Closing Ticket", "Ticket Closed");

    const response = await request;

    if (response.ok) {
      history.push(`/Project/Details/${projectId}`);
    }
  };

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project.id || "")}
      ></ProjectHeader>

      <div className="card-green-bg">
        <div className="card-content">
          <div className="ticket-details">
            <h2>Ticket Details</h2>
            <div className="row">
              <div className="col-12 col-md-8">
                <div>
                  <b>
                    <p>{ticket.name}</p>
                  </b>
                </div>
                <p>{ticket.requirements}</p>
              </div>
              <div className="col-12 col-md-4">
                <div className="row">
                  <div className="col">
                    <div>
                      <b>Status</b> <br />
                      <p>{ticket.status}</p>
                    </div>
                    <div>
                      <b>Due Date</b> <br />
                      {ticket.dueDate
                        ? new Date(ticket.dueDate).toDateString()
                        : "N/A"}
                    </div>
                    <div>
                      <b>Created</b> <br />
                      <p>{new Date(ticket.createdOn).toDateString()}</p>
                    </div>
                  </div>
                  <div className="col">
                    {ticket.updatedOn ? (
                      <>
                        <div>
                          <b>Updated On</b> <br />
                          {new Date(ticket.updatedOn).toDateString()}
                        </div>

                        <div>
                          <b>Updated By</b> <br />
                          {ticket.updatedBy ? ticket.updatedBy.name : "N/A"}
                        </div>
                      </>
                    ) : null}
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div className="ticket-response-list">
            <div className="row mt-4">
              <div className="col-12 col-md-8">
                <TicketReply
                  ticket={ticket}
                  projectId={projectId}
                  setTicket={(ticket: TicketDetails) =>
                    getIsMounted() && setTicket(ticket)
                  }
                />
                <ShowFor roles={["FieldManager", "Supervisor"]}>
                  <TicketWorkNotesEdit
                    ticket={ticket}
                    projectId={projectId}
                    setNotes={(notes: string) =>
                      setTicket({ ...ticket, workNotes: notes })
                    }
                  />
                </ShowFor>
                <br />
                <TicketMessages messages={ticket.messages} />
              </div>
              <div className="col-12 col-md-4">
                <TicketAttachments
                  ticket={ticket}
                  projectId={projectId}
                  setTicket={(ticket: TicketDetails) =>
                    getIsMounted() && setTicket(ticket)
                  }
                  attachments={ticket.attachments}
                />
              </div>
            </div>
          </div>
        </div>
        <div className="row justify-content-center pb-4">
          <Button
            className="btn-lg"
            color="secondary"
            onClick={closeTicket}
            disabled={ticket.completed || notification.pending}
          >
            Close Ticket
          </Button>
        </div>
      </div>
    </div>
  );
};
