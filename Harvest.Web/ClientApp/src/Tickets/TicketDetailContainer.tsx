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

  useEffect(() => {
    const cb = async () => {
      const response = await fetch(`/Project/Get/${projectId}`);

      if (response.ok) {
        const proj: Project = await response.json();
        setProject(proj);
      }
    };

    cb();
  }, [projectId]);

  useEffect(() => {
    const cb = async () => {
      const response = await fetch(`/Ticket/Get/${projectId}/${ticketId}`);

      if (response.ok) {
        const tick: TicketDetails = await response.json();
        setTicket(tick);
      }
    };

    cb();
  }, [ticketId, projectId]);

  if (project === undefined || ticket === undefined) {
    return <div>Loading...</div>;
  }

  const closeTicket = async () => {
    const request = fetch(
      `/Ticket/Close?projectId=${projectId}&ticketId=${ticketId}`,
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
        <div className="row justify-content-center">
          <div className="col-md-6 card-wrapper no-green mt-4 mb-4">
            <div className="card-content">
              <h2>Ticket Details</h2>
              <div className="row">
                <div className="col">
                  <p>
                    <b>Created</b> <br />
                    <p>{new Date(ticket.createdOn).toDateString()}</p>
                  </p>

                  <p>
                    <b>Due Date</b> <br />
                    {ticket.dueDate
                      ? new Date(ticket.dueDate).toDateString()
                      : "N/A"}
                  </p>
                  <p>
                    <b>Status</b> <br />
                    <p>{ticket.status}</p>
                  </p>
                </div>
                <div className="col">
                  {ticket.updatedOn ? (
                    <>
                      <p>
                        <b>Updated On</b> <br />
                        {new Date(ticket.updatedOn).toDateString()}
                      </p>

                      <p>
                        <b>Updated By</b> <br />
                        {ticket.updatedBy ? ticket.updatedBy.name : "N/A"}
                      </p>
                    </>
                  ) : null}
                </div>
              </div>
              <div className="row">
                <div className="col">
                  <p>
                    <b>Subject</b> <br />
                    <p>{ticket.name}</p>
                  </p>
                  <p>
                    <b>Details</b> <br />
                    <p>{ticket.requirements}</p>
                  </p>
                </div>
              </div>

              <ShowFor roles={["FieldManager", "Supervisor"]}>
                <TicketWorkNotesEdit
                  ticket={ticket}
                  projectId={projectId}
                  setNotes={(notes: string) =>
                    setTicket({ ...ticket, workNotes: notes })
                  }
                />
              </ShowFor>
              <hr />
              <TicketAttachments
                ticket={ticket}
                projectId={projectId}
                setTicket={(ticket: TicketDetails) => setTicket(ticket)}
                attachments={ticket.attachments}
              />
              <hr />

              <TicketMessages messages={ticket.messages} />
              <TicketReply
                ticket={ticket}
                projectId={projectId}
                setTicket={(ticket: TicketDetails) => setTicket(ticket)}
              />
            </div>
          </div>
        </div>
      </div>
      <div className="row justify-content-center">
        <Button
          className="btn-lg"
          color="primary"
          onClick={closeTicket}
          disabled={ticket.completed || notification.pending}
        >
          Close Ticket FOREVER!!!!
        </Button>
      </div>
    </div>
  );
};
