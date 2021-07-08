import React, { useState } from "react";
import { Button, FormGroup, Input, Label } from "reactstrap";
import { TicketDetails, TicketMessage } from "../types";

interface Props {
  ticket: TicketDetails;
  projectId: string;
  setTicket: (ticket: TicketDetails) => void;
}

export const TicketReply = (props: Props) => {
  const { ticket, setTicket, projectId } = props;
  const [ticketMessage, setTicketMessage] = useState<TicketMessage>({
    message: "",
  } as TicketMessage);

  const update = async () => {
    // TODO: validation

    const response = await fetch(
      `/Ticket/Reply?projectId=${props.projectId}&ticketId=${ticket.id}`,
      {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify(ticketMessage),
      }
    );

    if (response.ok) {
        const data = await response.json();
        ticket.messages.push(data);
        setTicket({ ...ticket });
        setTicketMessage({
            message: "",
        } as TicketMessage);
      alert("Reply saved.");
    } else {
      alert("Something went wrong, please try again");
    }
  };

  return (
    <>
      <h2>Reply</h2>
      <FormGroup>
        <Input
          type="textarea"
          name="text"
          id="message"
          value={ticketMessage.message}
          onChange={(e) =>
            setTicketMessage({ ...ticketMessage, message: e.target.value })
          }
        />
      </FormGroup>
      <div className="row justify-content-center">
        <Button className="btn-lg" color="primary" onClick={update}>
          Send
        </Button>
      </div>
      <div>DEBUG: {JSON.stringify(ticketMessage)}</div>
    </>
  );
};
