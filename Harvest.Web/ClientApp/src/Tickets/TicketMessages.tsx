import { useMemo } from "react";
import { TicketMessage } from "../types";

interface Props {
  messages: TicketMessage[];
}

export const TicketMessages = (props: Props) => {
  const ticketMessages = useMemo(() => props.messages, [props.messages]);

  return (
    <div>
      <h2>Conversation</h2>
      {ticketMessages === undefined || ticketMessages.length === 0 ? (
        <p> No Messages Yet!!!</p>
      ) : null}
      {ticketMessages.map((ticketMessage) => (
        <div className="ticket-response-card">
          <p key={`message-${ticketMessage.id}`}>
            <span className="ticket-responder">
              {ticketMessage.createdBy?.name}:
            </span>{" "}
            <br />
            {ticketMessage.message}
          </p>
        </div>
      ))}
    </div>
  );
};
