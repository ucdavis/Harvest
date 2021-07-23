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

interface RouteParams {
  projectId: string;
  ticketId: string;
}

export const TicketDetailContainer = () => {
  const { projectId, ticketId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();
  const [ticket, setTicket] = useState<TicketDetails>();
  const history = useHistory();

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
    const response = await fetch(
      `/Ticket/Close?projectId=${projectId}&ticketId=${ticketId}`,
      {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
      }
    );

    if (response.ok) {
      alert("Ticket Closed.");
      history.push(`/Project/Details/${projectId}`);
    } else {
      alert("Something went wrong, please try again");
    }
  };

  return (
    <div className="card-wrapper">
      <ProjectHeader
        project={project}
        title={"Field Request #" + (project.id || "")}
      ></ProjectHeader>
      <div className="card-content">
        <div className="card-head">
          <h2>Ticket Details</h2>
        </div>
        <p className="lede">Subject</p>
        <p>{ticket.name}</p>
        <p className="lede">Details</p>
        <p>{ticket.requirements}</p>
        <p className="lede">Status</p>
        <p>{ticket.status}</p>
        <p className="lede">Created</p>
        <p>{new Date(ticket.createdOn).toDateString()}</p>
        <p className="lede">Due Date</p>
        <p>
          {ticket.dueDate ? new Date(ticket.dueDate).toDateString() : "N/A"}
        </p>
        {ticket.updatedOn ? (
          <>
            <p className="lede">Updated On</p>
            <p>{new Date(ticket.updatedOn).toDateString()}</p>
            <p className="lede">Updated By</p>
            <p>{ticket.updatedBy ? ticket.updatedBy.name : "N/A"}</p>
          </>
        ) : null}
        <ShowFor roles={["FieldManager", "Supervisor"]}>
          <TicketWorkNotesEdit
            ticket={ticket}
            projectId={projectId}
            setNotes={(notes: string) =>
              setTicket({ ...ticket, workNotes: notes })
            }
          />
        </ShowFor>
        <TicketAttachments
          ticket={ticket}
          projectId={projectId}
          setTicket={(ticket: TicketDetails) => setTicket(ticket)}
          attachments={ticket.attachments}
        />
        <TicketMessages messages={ticket.messages} />
        <TicketReply
          ticket={ticket}
          projectId={projectId}
          setTicket={(ticket: TicketDetails) => setTicket(ticket)}
        />
      </div>
      <div className="row justify-content-center">
        <Button className="btn-lg" color="primary" onClick={closeTicket}>
          Close Ticket FOREVER!!!!
        </Button>
      </div>
    </div>
  );
};
