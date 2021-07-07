﻿import { TicketDetails } from "../types";
import { Button, FormGroup, Input, Label } from "reactstrap";

interface Props {
  ticket: TicketDetails;
  projectId: string;
  setNotes: (notes: string) => void;
}

export const TicketWorkNotesEdit = (props: Props) => {
  const { ticket } = props;

  const update = async () => {
    // TODO: validation

    const response = await fetch(
      `/Ticket/UpdateWorkNotes?projectId=${props.projectId}&ticketId=${props.ticket.id}`,
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
          onChange={(e) => props.setNotes(e.target.value)}
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
