import React, { useState } from "react";
import { Button, FormGroup, Input } from "reactstrap";
import { TicketDetails, TicketMessage } from "../types";
import { usePromiseNotification } from "../Util/Notifications";

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

  const [notification, setNotification] = usePromiseNotification();

  const update = async () => {
    // TODO: validation

    const request = fetch(
      `/Ticket/Reply?projectId=${projectId}&ticketId=${ticket.id}`,
      {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify(ticketMessage),
      }
    );
    
    setNotification(request, "Saving Reply", "Reply Saved");

    const response = await request;

    if (response.ok) {
      const data = await response.json();
      setTicket({ ...ticket, messages: [...ticket.messages, data] });
      setTicketMessage({
        message: "",
      } as TicketMessage);
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
        <Button
          className="btn"
          color="primary"
          onClick={update}
          disabled={ticket.completed || notification.pending}
        >
          Send
        </Button>
      </div>
    </>
  );
};
