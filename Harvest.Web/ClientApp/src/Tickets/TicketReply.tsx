import React, { useState } from "react";
import { Button, FormGroup, Input } from "reactstrap";
import { TicketDetails, TicketMessage } from "../types";
import { authenticatedFetch } from "../Util/Api";
import { usePromiseNotification } from "../Util/Notifications";
import { useIsMounted } from "../Shared/UseIsMounted";

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

  const getIsMounted = useIsMounted();
  const update = async () => {
    // TODO: validation

    const request = authenticatedFetch(
      `/api/Ticket/Reply?projectId=${projectId}&ticketId=${ticket.id}`,
      {
        method: "POST",
        body: JSON.stringify(ticketMessage),
      }
    );

    setNotification(request, "Saving Reply", "Reply Saved");

    const response = await request;

    if (response.ok) {
      const data = await response.json();
      if (getIsMounted()) {
        setTicket({ ...ticket, messages: [{ ...data }, ...ticket.messages] });
        setTicketMessage({
          message: "",
        } as TicketMessage);
      }
    }
  };

  return (
    <>
      <h3>Reply</h3>
      <FormGroup className="ticket-reply-form">
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
      <div className="row">
        <div className="col text-right">
          {" "}
          <Button
            className="btn btn-sm"
            color="primary"
            onClick={update}
            disabled={ticket.completed || notification.pending}
          >
            Send
          </Button>
        </div>
      </div>
    </>
  );
};
