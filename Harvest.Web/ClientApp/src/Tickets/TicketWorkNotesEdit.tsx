import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { Project, TicketDetails } from "../types";
import { useMemo } from "react";
import { Button, FormGroup, Input, Label } from "reactstrap";

interface Props {
  TicketDetails: TicketDetails;
  projectId: string;
}

export const TicketWorkNotesEdit = (props: Props) => {
    const [ticket, setTicket] = useState<TicketDetails>();
    const projectId = useMemo(() => props.projectId, [props.projectId]);
    const ticketId = useMemo(() => props.TicketDetails.id, [props.TicketDetails.id]);

    if (ticket === undefined) {
        return <div>ERROR...</div>;
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
        body: JSON.stringify(ticket),
      }
    );

    if (response.ok) {
      const data = await response.json();
    } else {
      alert("Something went wrong, please try again");
    }
  };

  return (
    <>
      <h2>Work Notes</h2>
      <FormGroup>
        <Label>Edit work notes</Label>
        <Input
          type="textarea"
          name="text"
          id="workNotes"
          value={ticket.workNotes}
          onChange={(e) => setTicket({ ...ticket, workNotes: e.target.value })}
        />
      </FormGroup>
      <div className="row justify-content-center">
        <Button className="btn-lg" color="primary" onClick={update}>
          Update Work Notes
        </Button>
      </div>
      <div>DEBUG: {JSON.stringify(ticket)}</div>
    </>
  );
};
