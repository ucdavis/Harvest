import { useMemo } from "react";
import { TicketMessage } from "../types";

interface Props {
  messages: TicketMessage[];
}

export const TicketMessages = (props: Props) => {
  const ticketMessages = useMemo(() => props.messages, [props.messages]);

  if (ticketMessages === undefined || ticketMessages.length === 0) {
    return (
      <div>
        <h2>Conversation</h2>
        <p>No Messages Yet!!!</p>
      </div>
    );
  }

  return (
    <div>
      <h2>Conversation</h2>
      <p>{ticketMessages[0].message}</p>
    </div>
  );
};
