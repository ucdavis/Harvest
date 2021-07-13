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
            <p key={`message-${ticketMessage.id}`}>
          {ticketMessage.message} from {ticketMessage.createdBy.name}
        </p>
      ))}
    </div>
  );
};
