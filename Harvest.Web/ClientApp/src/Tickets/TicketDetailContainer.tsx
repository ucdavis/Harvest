import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import {
  Project,
  TicketAttachment,
  TicketMessage,
  TicketDetails,
} from "../types";
import { ProjectHeader } from "../Shared/ProjectHeader";
import { TicketAttachments } from "./TicketAttachments";
import { TicketMessages } from "./TicketMessages";
import { ShowFor } from "../Shared/ShowFor";
import { TicketWorkNotesEdit } from "./TicketWorkNotesEdit";
import { Button, FormGroup, Input, Label } from "reactstrap";

interface RouteParams {
  projectId: string;
  ticketId: string;
}

export const TicketDetailContainer = () => {
  const { projectId, ticketId } = useParams<RouteParams>();
  const [project, setProject] = useState<Project>();
  const [ticket, setTicket] = useState<TicketDetails>();
  const [attachment, setAttachment] = useState<TicketAttachment>();
  const [message, setMessage] = useState<TicketMessage>();

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

  const update = async () => {
    // TODO: validation

    const response = await fetch(
      `/Ticket/UpdateWorkNotes?projectId=${projectId}&ticketId=${ticketId}`,
      {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify(ticket.workNotes),
      }
    );

    if (response.ok) {
      const data = await response.json();
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
        <ShowFor roles={["FieldManager", "Supervisor"]}>
          <h2>Work Notes</h2>
          <FormGroup>
            <Label>Edit work notes</Label>
            <Input
              type="textarea"
              name="text"
              id="workNotes"
              value={ticket.workNotes}
              onChange={(e) =>
                setTicket({ ...ticket, workNotes: e.target.value })
              }
            />
          </FormGroup>
          <div className="row justify-content-center">
            <Button className="btn-lg" color="primary" onClick={update}>
              Update Work Notes
            </Button>
          </div>
          <div>DEBUG: {JSON.stringify(ticket.workNotes)}</div>
          <TicketWorkNotesEdit
            ticket={ticket}
            projectId={projectId}
            setNotes={(notes: string) => setTicket({ ...ticket, workNotes: notes })}
          ></TicketWorkNotesEdit>
        </ShowFor>
        <TicketAttachments attachments={ticket.attachments}></TicketAttachments>
        <TicketMessages messages={ticket.messages}></TicketMessages>
      </div>
    </div>
  );
};
